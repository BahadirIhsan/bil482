using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Usta teklif verdi (UC-03). Müşteriye NEW_OFFER ve offer.created.</summary>
public sealed record OfferSubmittedEvent(
    Guid OfferId,
    Guid ReportId,
    Guid MechanicId,
    Guid CustomerId,
    decimal TotalPrice,
    DateTimeOffset OccurredAt) : IDomainEvent;
