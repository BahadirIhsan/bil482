using FluentAssertions;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Reports;

namespace IndustryApp.UnitTests.Domain;

/// <summary>PAT-04.1 State: izin verilen ve reddedilen rapor geçişleri.</summary>
public sealed class ReportStateTests
{
    // TC-04.2.1
    [Fact]
    public void OpenState_SelectOffer_MovesToOfferSelected()
    {
        var report = DomainTestData.OpenReport();
        var offer = DomainTestData.OfferFor(report, Guid.NewGuid());
        var selectedAt = DomainTestData.Now.AddMinutes(5);

        report.SelectOffer(offer, selectedAt);

        report.Status.Should().Be(ReportStatus.OfferSelected);
        report.SelectedOfferId.Should().Be(offer.Id);
        report.OfferSelectedAt.Should().Be(selectedAt);
        report.CanReceiveOffers.Should().BeFalse();
        report.DomainEvents.OfType<OfferSelectedEvent>().Should().ContainSingle(e => e.SelectedOfferId == offer.Id);
    }

    // TC-04.2.2
    [Fact]
    public void CompletedState_SelectOffer_Throws()
    {
        var report = DomainTestData.OpenReport();
        var winner = DomainTestData.OfferFor(report, Guid.NewGuid());
        var other = DomainTestData.OfferFor(report, Guid.NewGuid(), labor: 4000m);
        report.SelectOffer(winner, DomainTestData.Now.AddMinutes(5));
        report.MarkCompleted(DomainTestData.Now.AddDays(3));

        var act = () => report.SelectOffer(other, DomainTestData.Now.AddDays(4));

        act.Should().Throw<DomainException>().Which.Code.Should().Be(ErrorCodes.ReportInvalidState);
        report.Status.Should().Be(ReportStatus.Completed);
        report.SelectedOfferId.Should().Be(winner.Id);
    }
}
