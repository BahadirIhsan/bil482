using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Usta bekleyen teklifini geri çekti (UC-03). Müşteriye offer.withdrawn.</summary>
public sealed record OfferWithdrawnEvent(
    Guid OfferId,
    Guid ReportId,
    Guid CustomerId,
    DateTimeOffset OccurredAt) : IDomainEvent;
