using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Müşteri seçimi süre içinde geri aldı (UC-04). Teklif veren ustalara SELECTION_UNDONE.</summary>
public sealed record OfferSelectionUndoneEvent(
    Guid ReportId,
    IReadOnlyCollection<Guid> MechanicIds,
    DateTimeOffset OccurredAt) : IDomainEvent;
