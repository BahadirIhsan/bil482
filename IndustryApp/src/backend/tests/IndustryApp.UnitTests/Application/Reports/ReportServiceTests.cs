using FluentAssertions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Reports;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using IndustryApp.UnitTests.Domain;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IndustryApp.UnitTests.Application.Reports;

/// <summary>FR-04.5: tamamlama ve iptal State üzerinden yapılır.</summary>
public sealed class ReportServiceTests
{
    private readonly ServiceTestContext _ctx = new();
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _service = new ReportService(
            _ctx.Reports,
            _ctx.History,
            Substitute.For<IVehicleTypeRepository>(),
            Substitute.For<IDamageCategoryRepository>(),
            _ctx.UnitOfWork,
            _ctx.CurrentUser,
            _ctx.Clock,
            Options.Create(new OffersOptions { SelectionUndoWindowMinutes = 10 }));
    }

    [Fact]
    public async Task Cancel_OpenReport_ClosesPendingOffers()
    {
        var report = DomainTestData.OpenReport(_ctx.UserId);
        var first = DomainTestData.OfferFor(report, Guid.NewGuid());
        var second = DomainTestData.OfferFor(report, Guid.NewGuid(), labor: 4100m);
        _ctx.Returns(report);

        var result = await _service.CancelAsync(report.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Cancelled);
        new[] { first, second }.Should().OnlyContain(o => o.Status == OfferStatus.Closed);
        _ctx.HistoryEntries.Should().ContainSingle(h => h.CommandName == ReportCommandNames.CancelReport);
    }

    [Fact]
    public async Task Complete_OpenReport_IsRejectedByState()
    {
        var report = DomainTestData.OpenReport(_ctx.UserId);
        _ctx.Returns(report);

        var act = () => _service.CompleteAsync(report.Id, CancellationToken.None);

        (await act.Should().ThrowAsync<DomainException>()).Which.Code.Should().Be(ErrorCodes.ReportInvalidState);
        await _ctx.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Complete_NotOwner_ReturnsForbidden()
    {
        var report = DomainTestData.OpenReport(Guid.NewGuid());
        _ctx.Returns(report);

        var result = await _service.CompleteAsync(report.Id, CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.NotReportOwner);
    }
}
