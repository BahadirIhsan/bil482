namespace IndustryApp.Domain.Reports.States;

/// <summary>Expired: son durum; hiçbir işlemi kabul etmez.</summary>
public sealed class ExpiredState : ReportStateBase
{
    public override ReportStatus Status => ReportStatus.Expired;
}
