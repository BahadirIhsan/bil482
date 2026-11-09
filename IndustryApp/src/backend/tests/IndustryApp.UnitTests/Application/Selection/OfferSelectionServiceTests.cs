using FluentAssertions;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Selection;
using IndustryApp.Application.Selection.Contracts;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using IndustryApp.UnitTests.Domain;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IndustryApp.UnitTests.Application.Selection;

/// <summary>UC-04 seçim servisi: sahiplik, geçmiş kaydı, geri alma akışı (FR-04.2, FR-04.3).</summary>
public sealed class OfferSelectionServiceTests
{
    private readonly ServiceTestContext _ctx = new();
    private readonly OfferSelectionService _service;

    public OfferSelectionServiceTests()
    {
        var options = Options.Create(new OffersOptions { SelectionUndoWindowMinutes = 10 });
        _service = new OfferSelectionService(
            _ctx.Reports, _ctx.History, new ReportCommandFactory(options), _ctx.UnitOfWork, _ctx.CurrentUser, _ctx.Clock, options);
    }

    // TC-04.1.1
    [Fact]
    public async Task SelectOffer_NotOwner_ReturnsForbidden()
    {
        var report = DomainTestData.OpenReport(customerId: Guid.NewGuid());
        var offer = DomainTestData.OfferFor(report, Guid.NewGuid());
        _ctx.Returns(report);

        var result = await _service.SelectAsync(new SelectOfferRequest(report.Id, offer.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(ErrorCodes.NotReportOwner);
        report.Status.Should().Be(ReportStatus.Open);
        await _ctx.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SelectOffer_Owner_SavesSnapshotAndReturnsUndoDeadline()
    {
        var report = DomainTestData.OpenReport(customerId: _ctx.UserId);
        var chosen = DomainTestData.OfferFor(report, Guid.NewGuid());
        var other = DomainTestData.OfferFor(report, Guid.NewGuid(), labor: 4200m);
        _ctx.Returns(report);

        var result = await _service.SelectAsync(new SelectOfferRequest(report.Id, chosen.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UndoDeadline.Should().Be(_ctx.Now.AddMinutes(10));
        chosen.Status.Should().Be(OfferStatus.Accepted);
        other.Status.Should().Be(OfferStatus.NotSelected);

        var entry = _ctx.HistoryEntries.Should().ContainSingle().Subject;
        entry.CommandName.Should().Be(ReportCommandNames.SelectOffer);
        entry.FromStatus.Should().Be(ReportStatus.Open);
        entry.ToStatus.Should().Be(ReportStatus.OfferSelected);
        entry.ActorUserId.Should().Be(_ctx.UserId);
        entry.ReadSnapshot()!.NotSelectedOfferIds.Should().Equal(other.Id);
        await _ctx.UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SelectOffer_WithdrawnOffer_ReturnsOfferNotAvailable()
    {
        var report = DomainTestData.OpenReport(customerId: _ctx.UserId);
        var offer = DomainTestData.OfferFor(report, Guid.NewGuid());
        offer.Withdraw(report, DomainTestData.Now.AddMinutes(1));
        _ctx.Returns(report);

        var result = await _service.SelectAsync(new SelectOfferRequest(report.Id, offer.Id), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.OfferNotAvailable);
    }

    [Fact]
    public async Task SelectThenUndo_WithinWindow_ReopensAllOffers()
    {
        var report = DomainTestData.OpenReport(customerId: _ctx.UserId);
        var chosen = DomainTestData.OfferFor(report, Guid.NewGuid());
        var other = DomainTestData.OfferFor(report, Guid.NewGuid(), labor: 4200m);
        _ctx.Returns(report);
        await _service.SelectAsync(new SelectOfferRequest(report.Id, chosen.Id), CancellationToken.None);

        _ctx.Now = _ctx.Now.AddMinutes(5);
        var result = await _service.UndoAsync(new UndoSelectionRequest(report.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ReportStatus.Open);
        result.Value.CanUndo.Should().BeFalse();
        chosen.Status.Should().Be(OfferStatus.Pending);
        other.Status.Should().Be(OfferStatus.Pending);
        _ctx.HistoryEntries.Select(h => h.CommandName)
            .Should().Equal(ReportCommandNames.SelectOffer, ReportCommandNames.UndoSelectOffer);
    }

    [Fact]
    public async Task Undo_WithoutSelection_ReturnsConflict()
    {
        var report = DomainTestData.OpenReport(customerId: _ctx.UserId);
        _ctx.Returns(report);

        var result = await _service.UndoAsync(new UndoSelectionRequest(report.Id), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.ReportInvalidState);
        await _ctx.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStatus_UnrelatedUser_ReturnsNotMatched()
    {
        var report = DomainTestData.OpenReport(customerId: Guid.NewGuid());
        _ctx.Returns(report);

        var result = await _service.GetAsync(report.Id, CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.NotMatched);
    }
}
