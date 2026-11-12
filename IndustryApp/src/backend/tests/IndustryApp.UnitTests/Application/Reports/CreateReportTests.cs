using FluentAssertions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common.Exceptions;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Reports;
using IndustryApp.Application.Reports.Contracts;
using IndustryApp.Application.Reports.Photos;
using IndustryApp.Domain.Catalog;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Reports;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IndustryApp.UnitTests.Application.Reports;

/// <summary>UC-02: rapor Builder ile kurulur, fotoğraflar kalıcı anahtara taşınır, hata olursa geri taşınır.</summary>
public sealed class CreateReportTests
{
    private const int CategoryId = 12;

    private readonly ServiceTestContext _ctx = new();
    private readonly IVehicleTypeRepository _vehicleTypes = Substitute.For<IVehicleTypeRepository>();
    private readonly IDamageCategoryRepository _categories = Substitute.For<IDamageCategoryRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IImageInfoReader _imageInfo = Substitute.For<IImageInfoReader>();
    private readonly List<AccidentReport> _added = new();
    private readonly ReportService _service;

    public CreateReportTests()
    {
        _vehicleTypes.ExistsAsync(Arg.Any<short>(), Arg.Any<CancellationToken>()).Returns(true);
        _categories.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<IReadOnlyCollection<int>>().Contains(CategoryId)
                ? [DamageCategory.Create("BODY_FRONT_BUMPER", "Ön tampon", 1).WithId(CategoryId)]
                : []);
        _storage.GetSizeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(180_000L);
        _storage.OpenReadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(_ => new MemoryStream([1]));
        _imageInfo.ReadSizeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns((1920, 1440));
        _ctx.Reports.When(r => r.AddAsync(Arg.Any<AccidentReport>(), Arg.Any<CancellationToken>()))
            .Do(call => _added.Add(call.Arg<AccidentReport>()));

        _service = new ReportService(
            _ctx.Reports, _ctx.History, _vehicleTypes, _categories, _storage, _imageInfo,
            _ctx.UnitOfWork, _ctx.CurrentUser, _ctx.Clock,
            Options.Create(new OffersOptions { SelectionUndoWindowMinutes = 10 }),
            Options.Create(new ReportsOptions { ExpireAfterHours = 72, MaxOpenPerCustomer = 3 }),
            NullLogger<ReportService>.Instance);
    }

    [Fact]
    public async Task CreateReport_Valid_SavesOpenReportAndMovesPhotos()
    {
        var keys = new[] { TempKey(), TempKey() };

        var result = await _service.CreateAsync(Request(keys), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var report = _added.Should().ContainSingle().Subject;
        report.Status.Should().Be(ReportStatus.Open);
        report.ExpiresAt.Should().Be(_ctx.Now.AddHours(72));
        report.Photos.Should().HaveCount(2).And.OnlyContain(p => p.StorageKey.StartsWith($"reports/{_ctx.Now:yyyy}/{_ctx.Now:MM}/{report.Id}/"));
        report.Photos.Select(p => (p.Width, p.Height, p.SizeBytes)).Should().AllBeEquivalentTo((1920, 1440, 180_000));
        report.DomainEvents.Should().ContainSingle(e => e is AccidentReportCreatedEvent);
        foreach (var key in keys)
            await _storage.Received(1).MoveAsync(key, Arg.Is<string>(k => k.StartsWith("reports/")), Arg.Any<CancellationToken>());
        _ctx.HistoryEntries.Should().ContainSingle(h => h.CommandName == ReportCommandNames.CreateReport && h.FromStatus == null);
        await _ctx.UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // TC-02.3.3
    [Fact]
    public async Task CreateReport_ForeignPhotoKey_ReturnsInvalidPhotoKey()
    {
        var foreignKey = $"temp/{Guid.NewGuid()}/{Guid.NewGuid()}.jpg";

        var result = await _service.CreateAsync(Request([TempKey(), foreignKey]), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.InvalidPhotoKey);
        await _storage.DidNotReceiveWithAnyArgs().MoveAsync(default!, default!, default);
        await _ctx.UnitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task CreateReport_MissingTemporaryFile_ReturnsInvalidPhotoKey()
    {
        var key = TempKey();
        _storage.GetSizeAsync(key, Arg.Any<CancellationToken>()).Returns((long?)null);

        var result = await _service.CreateAsync(Request([key]), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.InvalidPhotoKey);
    }

    [Fact]
    public async Task CreateReport_NoPhoto_ReturnsReportIncompleteFromBuilder()
    {
        var result = await _service.CreateAsync(Request([]), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.ReportIncomplete);
        result.Error.Message.Should().Contain("fotoğraf");
    }

    [Fact]
    public async Task CreateReport_InactiveOrUnknownCategory_ReturnsCategoryInactive()
    {
        var request = Request([TempKey()]) with { Damages = [new ReportDamageInput(999, DamageSeverity.Heavy)] };

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.CategoryInactive);
    }

    [Fact]
    public async Task CreateReport_ThreeOpenReports_ReturnsLimitReached()
    {
        _ctx.Reports.CountOpenByCustomerAsync(_ctx.UserId, Arg.Any<CancellationToken>()).Returns(3);

        var result = await _service.CreateAsync(Request([TempKey()]), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.OpenReportLimitReached);
    }

    [Fact]
    public async Task CreateReport_SaveFails_MovesPhotosBack()
    {
        var keys = new[] { TempKey(), TempKey() };
        _ctx.UnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new ConflictException(ErrorCodes.UniqueViolation, "çakışma"));

        var act = () => _service.CreateAsync(Request(keys), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        foreach (var key in keys)
            await _storage.Received(1).MoveAsync(Arg.Is<string>(k => k.StartsWith("reports/")), key, Arg.Any<CancellationToken>());
    }

    private string TempKey() => $"temp/{_ctx.UserId}/{Guid.NewGuid()}.jpg";

    private static TestRequest Request(IReadOnlyList<string> photoKeys) => new(
        VehicleTypeId: 1,
        Description: "Arka tampon ve stop lambası kırık.",
        City: "Ankara",
        District: "Çankaya",
        AddressText: null,
        Latitude: 39.92,
        Longitude: 32.85,
        IsDeviceLocation: false,
        Damages: [new ReportDamageInput(CategoryId, DamageSeverity.Medium)],
        PhotoKeys: photoKeys);

    private sealed record TestRequest(
        short VehicleTypeId,
        string? Description,
        string? City,
        string? District,
        string? AddressText,
        double? Latitude,
        double? Longitude,
        bool IsDeviceLocation,
        IReadOnlyList<ReportDamageInput>? Damages,
        IReadOnlyList<string>? PhotoKeys) : ICreateReportRequest;
}
