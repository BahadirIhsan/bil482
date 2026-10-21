namespace IndustryApp.Domain.Accounts;

/// <summary>Usta hesabının onay durumu. Yalnızca Approved olan usta eşleştirmeye girer (FR-01.3).</summary>
public enum MechanicApprovalStatus
{
    PendingApproval,
    Approved,
    Rejected,
    Suspended
}
