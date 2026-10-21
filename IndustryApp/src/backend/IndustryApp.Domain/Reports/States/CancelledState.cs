namespace IndustryApp.Domain.Reports.States;

/// <summary>Cancelled: son durum; hiçbir işlemi kabul etmez.</summary>
public sealed class CancelledState : ReportStateBase
{
    public override ReportStatus Status => ReportStatus.Cancelled;
}
