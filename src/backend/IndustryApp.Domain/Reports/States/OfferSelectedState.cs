using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Reports.States;

/// <summary>OfferSelected: seçim süre içinde geri alınabilir, iş tamamlandı işaretlenebilir, rapor iptal edilebilir.</summary>
public sealed class OfferSelectedState : ReportStateBase
{
    public override ReportStatus Status => ReportStatus.OfferSelected;

    public override void UndoSelection(AccidentReport report, DateTimeOffset now, TimeSpan window)
    {
        if (report.OfferSelectedAt is null || now - report.OfferSelectedAt.Value > window)
            throw new DomainException(ErrorCodes.UndoWindowExpired, "Seçimi geri alma süresi doldu.");

        report.ApplyTransition(ReportStatus.Open, now);
    }

    public override void Complete(AccidentReport report, DateTimeOffset now) =>
        report.ApplyTransition(ReportStatus.Completed, now);

    public override void Cancel(AccidentReport report, DateTimeOffset now) =>
        report.ApplyTransition(ReportStatus.Cancelled, now);
}
