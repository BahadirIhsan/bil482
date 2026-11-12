using System.Diagnostics;
using FluentAssertions;
using IndustryApp.Application.Reports;
using IndustryApp.Application.Reports.Contracts;
using IndustryApp.Application.Reports.Photos;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Reports;
using IndustryApp.Infrastructure.Events;
using IndustryApp.Infrastructure.Persistence;
using IndustryApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace IndustryApp.IntegrationTests.Uc02;

/// <summary>UC-02 gerçek PostgreSQL ve disk üzerinde: fotoğraf yükleme, rapor oluşturma, fotoğraf erişimi.</summary>
[Collection(PostgresCollection.Name)]
public sealed class CreateReportIntegrationTests(PostgresFixture fixture)
{
    // TC-02.3.2
    [Fact]
    public async Task CreateReport_ReturnsBeforeMatchingCompletes()
    {
        var customerId = await CreateCustomerAsync();
        var dispatcher = fixture.Services.GetRequiredService<InProcessEventDispatcher>();

        // Isınma: ilk çağrı EF modelini ve sorgu derlemesini hazırlar; ölçülen çağrı ikincisidir.
        (await CreateReportAsync(customerId)).IsSuccess.Should().BeTrue();

        var stopwatch = Stopwatch.StartNew();
        var result = await CreateReportAsync(customerId);
        stopwatch.Stop();

        result.IsSuccess.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "rapor oluşturma eşleştirmeyi beklemez (NFR-P2)");

        // Eşleştirme yanıt döndüğünde çalışmamıştır: olay kuyrukta bekler, report_matches boştur.
        var queued = Drain(dispatcher).OfType<AccidentReportCreatedEvent>().Where(e => e.ReportId == result.Value.Id).ToList();
        queued.Should().ContainSingle().Which.RootCategoryIds.Should().NotBeEmpty();

        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.ReportMatches.CountAsync(m => m.ReportId == result.Value.Id)).Should().Be(0);
        var report = await db.AccidentReports.Include(r => r.Photos).SingleAsync(r => r.Id == result.Value.Id);
        report.Status.Should().Be(ReportStatus.Open);
        report.Photos.Should().ContainSingle()
            .Which.StorageKey.Should().StartWith("reports/").And.Contain(report.Id.ToString());
        File.Exists(Path.Combine(fixture.StorageRoot, report.Photos.Single().StorageKey)).Should().BeTrue();
        Directory.GetFiles(Path.Combine(fixture.StorageRoot, "temp", customerId.ToString())).Should().BeEmpty();
    }

    [Fact]
    public async Task Photo_OwnerCanOpen_OtherCustomerIsForbidden()
    {
        var owner = await CreateCustomerAsync();
        var stranger = await CreateCustomerAsync();
        var created = await CreateReportAsync(owner);
        Guid photoId;
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            photoId = await scope.ServiceProvider.GetRequiredService<AppDbContext>().ReportPhotos
                .Where(p => p.ReportId == created.Value.Id).Select(p => p.Id).SingleAsync();
        }

        await using (var scope = fixture.ScopeFor(owner))
        {
            var photo = await scope.ServiceProvider.GetRequiredService<IPhotoService>().OpenAsync(photoId, CancellationToken.None);
            photo.IsSuccess.Should().BeTrue();
            await using var content = photo.Value.Content;
            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer);
            buffer.ToArray().AsSpan(0, 3).ToArray().Should().Equal(0xFF, 0xD8, 0xFF);
            photo.Value.ContentType.Should().Be("image/jpeg");
        }

        await using (var scope = fixture.ScopeFor(stranger))
        {
            scope.ServiceProvider.GetRequiredService<TestCurrentUser>().Roles = [RoleNames.Customer];
            var photo = await scope.ServiceProvider.GetRequiredService<IPhotoService>().OpenAsync(photoId, CancellationToken.None);
            photo.Error!.Code.Should().Be(ErrorCodes.NotReportOwner);
        }
    }

    [Fact]
    public async Task ListMine_ReturnsOnlyOwnReportsNewestFirst()
    {
        var customerId = await CreateCustomerAsync();
        var first = await CreateReportAsync(customerId);
        var second = await CreateReportAsync(customerId);
        await CreateReportAsync(await CreateCustomerAsync());

        await using var scope = fixture.ScopeFor(customerId);
        var list = await scope.ServiceProvider.GetRequiredService<IReportService>()
            .ListMineAsync(new Paging(1, 20), CancellationToken.None);

        list.Value.Items.Select(r => r.Id).Should().Equal(second.Value.Id, first.Value.Id);
        list.Value.Items.Should().OnlyContain(r => r.Status == ReportStatus.Open && r.OfferCount == 0 && r.VehicleTypeName != "");
    }

    private async Task<IndustryApp.Application.Common.Result<CreateReportResponse>> CreateReportAsync(Guid customerId)
    {
        await using var scope = fixture.ScopeFor(customerId);
        var photos = scope.ServiceProvider.GetRequiredService<IPhotoService>();
        var upload = await photos.UploadTemporaryAsync(new PhotoUpload(Png(2400, 1600)), CancellationToken.None);
        upload.IsSuccess.Should().BeTrue();
        upload.Value.Width.Should().Be(1920);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var vehicleTypeId = await db.VehicleTypes.Where(v => v.Code == "CAR").Select(v => v.Id).SingleAsync();
        var categoryId = await db.DamageCategories.Where(c => c.Level == 3 && c.IsActive).OrderBy(c => c.Id).Select(c => c.Id).FirstAsync();

        return await scope.ServiceProvider.GetRequiredService<IReportService>().CreateAsync(new CreateRequest(
            vehicleTypeId, "Ön tampon çatladı.", "Ankara", "Çankaya", null, 39.92, 32.85, true,
            [new ReportDamageInput(categoryId, DamageSeverity.Medium)], [upload.Value.TempKey]), CancellationToken.None);
    }

    private async Task<Guid> CreateCustomerAsync()
    {
        var id = Guid.NewGuid();
        var email = $"{id:N}@test.local";
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO users (id, full_name, user_name, normalized_user_name, email, normalized_email, phone_number)
            VALUES ({id}, 'UC-02 Müşteri', {email}, {email.ToUpperInvariant()}, {email}, {email.ToUpperInvariant()}, '05321112233')
            """);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO customer_profiles (user_id, city, district) VALUES ({id}, 'Ankara', 'Çankaya')");
        return id;
    }

    private static List<IDomainEvent> Drain(InProcessEventDispatcher dispatcher)
    {
        var events = new List<IDomainEvent>();
        while (dispatcher.Reader.TryRead(out var e))
            events.Add(e);
        return events;
    }

    private static byte[] Png(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
            canvas.Clear(SKColors.IndianRed);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private sealed class PhotoUpload(byte[] bytes) : IUploadPhotoRequest
    {
        public Stream Content { get; } = new MemoryStream(bytes);

        public string FileName => "kaza.png";

        public string ContentType => "image/png";

        public long Length => bytes.Length;
    }

    private sealed record CreateRequest(
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

    private sealed record Paging(int Page, int PageSize) : IndustryApp.Application.Common.IPagedQuery;
}
