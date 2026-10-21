using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Usta bekleyen teklifini güncelledi (UC-03). Müşteriye offer.updated.</summary>
public sealed record OfferUpdatedEvent(
    Guid OfferId,
    Guid ReportId,
    Guid CustomerId,
    decimal TotalPrice,
    DateTimeOffset OccurredAt) : IDomainEvent;
