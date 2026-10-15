using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Müşteri raporu iptal etti (UC-04). Teklif vermiş ustalara REPORT_CANCELLED.</summary>
public sealed record ReportCancelledEvent(
    Guid ReportId,
    IReadOnlyCollection<Guid> MechanicIdsWithOffers,
    DateTimeOffset OccurredAt) : IDomainEvent;
