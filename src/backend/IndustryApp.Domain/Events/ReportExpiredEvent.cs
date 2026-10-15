using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Teklif seçilmeden süre doldu (UC-02, FR-02.4). Müşteriye REPORT_EXPIRED.</summary>
public sealed record ReportExpiredEvent(
    Guid ReportId,
    Guid CustomerId,
    DateTimeOffset OccurredAt) : IDomainEvent;
