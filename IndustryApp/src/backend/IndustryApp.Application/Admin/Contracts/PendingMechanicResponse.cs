using IndustryApp.Domain.Accounts;

namespace IndustryApp.Application.Admin.Contracts;

/// <summary>Admin onay listesindeki usta satırı (YA-01).</summary>
public sealed record PendingMechanicResponse(
    Guid MechanicId,
    string FullName,
    string Email,
    string WorkshopName,
    string TaxNumber,
    string City,
    string District,
    string Address,
    MechanicApprovalStatus ApprovalStatus,
    string? RejectionReason,
    IReadOnlyList<int> SpecialtyCategoryIds,
    DateTimeOffset CreatedAt);
