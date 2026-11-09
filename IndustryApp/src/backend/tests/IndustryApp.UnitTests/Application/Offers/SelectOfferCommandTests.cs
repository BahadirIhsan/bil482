using FluentAssertions;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using IndustryApp.UnitTests.Domain;
using Microsoft.Extensions.Options;

namespace IndustryApp.UnitTests.Application.Offers;

/// <summary>PAT-04.2 Command: seçim yürütülür, snapshot'tan yeniden kurulur ve süre içinde birebir geri alınır.</summary>
public sealed class SelectOfferCommandTests
{
    private static readonly Guid Actor = Guid.NewGuid();
    private static readonly DateTimeOffset SelectedAt = DomainTestData.Now.AddMinutes(30);

    private readonly ReportCommandFactory _factory =
        new(Options.Create(new OffersOptions { SelectionUndoWindowMinutes = 10 }));

    [Fact]
    public void Execute_AcceptsChosen_AndMarksOtherPendingOffersNotSelected()
    {
        var (report, chosen, others) = ReportWithThreeOffers();

        var command = _factory.CreateSelectOffer(report, chosen, Actor, SelectedAt);
        command.Execute();

        report.Status.Should().Be(ReportStatus.OfferSelected);
        report.SelectedOfferId.Should().Be(chosen.Id);
        chosen.Status.Should().Be(OfferStatus.Accepted);
        others.Should().OnlyContain(o => o.Status == OfferStatus.NotSelected);
        command.Name.Should().Be(ReportCommandNames.SelectOffer);
        command.CreateSnapshot().Should().BeEquivalentTo(
            new ReportCommandSnapshot(chosen.Id, others.Select(o => o.Id).ToArray()));
    }

    // TC-04.3.1
    [Fact]
    public void Undo_WithinWindow_RestoresPendingOffers()
    {
        var (report, chosen, others) = ReportWithThreeOffers();
        var history = ExecuteAndRecord(report, chosen);

        // Ayrı bir HTTP isteği gibi: komut yalnızca geçmiş kaydından yeniden kurulur.
        var restored = _factory.Restore(report, history, report.Offers.ToList(), Actor, SelectedAt.AddMinutes(9));
        restored.Undo();

        report.Status.Should().Be(ReportStatus.Open);
        report.SelectedOfferId.Should().BeNull();
        report.OfferSelectedAt.Should().BeNull();
        report.Offers.Should().OnlyContain(o => o.Status == OfferStatus.Pending);
        report.DomainEvents.OfType<OfferSelectionUndoneEvent>().Should().ContainSingle()
            .Which.MechanicIds.Should().BeEquivalentTo(new[] { chosen.MechanicId }.Concat(others.Select(o => o.MechanicId)));
    }

    // TC-04.3.2
    [Fact]
    public void Undo_AfterWindow_Throws()
    {
        var (report, chosen, others) = ReportWithThreeOffers();
        var history = ExecuteAndRecord(report, chosen);

        var restored = _factory.Restore(report, history, report.Offers.ToList(), Actor, SelectedAt.AddMinutes(10).AddSeconds(1));
        var act = () => restored.Undo();

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.UndoWindowExpired);
        report.Status.Should().Be(ReportStatus.OfferSelected);
        chosen.Status.Should().Be(OfferStatus.Accepted);
        others.Should().OnlyContain(o => o.Status == OfferStatus.NotSelected);
    }

    [Fact]
    public void Execute_OnCompletedReport_IsRejectedByState()
    {
        var (report, chosen, others) = ReportWithThreeOffers();
        _factory.CreateSelectOffer(report, chosen, Actor, SelectedAt).Execute();
        report.MarkCompleted(SelectedAt.AddDays(2));

        var act = () => _factory.CreateSelectOffer(report, others[0], Actor, SelectedAt.AddDays(3)).Execute();

        act.Should().Throw<DomainException>();
        report.Status.Should().Be(ReportStatus.Completed);
    }

    [Fact]
    public void Restore_WithNonUndoableEntry_Throws()
    {
        var (report, _, _) = ReportWithThreeOffers();
        var entry = ReportStatusHistory.Create(
            report.Id, ReportStatus.OfferSelected, ReportStatus.Completed, ReportCommandNames.CompleteReport, Actor, null, SelectedAt);

        var act = () => _factory.Restore(report, entry, report.Offers.ToList(), Actor, SelectedAt);

        act.Should().Throw<InvalidOperationException>();
    }

    private ReportStatusHistory ExecuteAndRecord(AccidentReport report, Offer chosen)
    {
        var command = _factory.CreateSelectOffer(report, chosen, Actor, SelectedAt);
        command.Execute();
        report.ClearDomainEvents();
        return ReportStatusHistory.Create(
            report.Id, ReportStatus.Open, report.Status, command.Name, command.ActorId, command.CreateSnapshot(), SelectedAt);
    }

    private static (AccidentReport Report, Offer Chosen, IReadOnlyList<Offer> Others) ReportWithThreeOffers()
    {
        var report = DomainTestData.OpenReport();
        var chosen = DomainTestData.OfferFor(report, Guid.NewGuid(), labor: 4000m);
        var second = DomainTestData.OfferFor(report, Guid.NewGuid(), labor: 4500m);
        var third = DomainTestData.OfferFor(report, Guid.NewGuid(), labor: 5000m);
        return (report, chosen, new[] { second, third });
    }
}
