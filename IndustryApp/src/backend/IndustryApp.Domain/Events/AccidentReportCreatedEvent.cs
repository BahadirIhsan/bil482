using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Rapor kaydedildi (UC-02). Dinleyen: Matching → otomatik eşleştirme.</summary>
public sealed record AccidentReportCreatedEvent(
    Guid ReportId,
    Guid CustomerId,
    string City,
    double Latitude,
    double Longitude,
    IReadOnlyCollection<int> RootCategoryIds,
    DateTimeOffset OccurredAt) : IDomainEvent;
