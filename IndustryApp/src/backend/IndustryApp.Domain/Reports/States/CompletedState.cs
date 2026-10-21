namespace IndustryApp.Domain.Reports.States;

/// <summary>Completed: son durum; hiçbir işlemi kabul etmez.</summary>
public sealed class CompletedState : ReportStateBase
{
    public override ReportStatus Status => ReportStatus.Completed;
}
