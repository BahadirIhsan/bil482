using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Müşteri işi tamamlandı olarak işaretledi (UC-04). Ustaya JOB_COMPLETED.</summary>
public sealed record ReportCompletedEvent(
    Guid ReportId,
    Guid WinnerMechanicId,
    DateTimeOffset OccurredAt) : IDomainEvent;
