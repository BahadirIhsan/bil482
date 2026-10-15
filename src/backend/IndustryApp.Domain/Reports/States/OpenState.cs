using IndustryApp.Domain.Offers;

namespace IndustryApp.Domain.Reports.States;

/// <summary>Open: teklif alır; teklif seçilebilir, iptal edilebilir, süresi dolabilir.</summary>
public sealed class OpenState : ReportStateBase
{
    public override ReportStatus Status => ReportStatus.Open;

    public override bool CanReceiveOffers => true;

    public override void SelectOffer(AccidentReport report, Offer offer, DateTimeOffset now) =>
        report.ApplyTransition(ReportStatus.OfferSelected, now, selectedOfferId: offer.Id);

    public override void Cancel(AccidentReport report, DateTimeOffset now) =>
        report.ApplyTransition(ReportStatus.Cancelled, now);

    public override void Expire(AccidentReport report, DateTimeOffset now) =>
        report.ApplyTransition(ReportStatus.Expired, now);
}
