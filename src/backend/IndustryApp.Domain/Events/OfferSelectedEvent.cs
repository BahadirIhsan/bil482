using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Müşteri teklif seçti (UC-04). Kazanana OFFER_SELECTED, diğerlerine OFFER_NOT_SELECTED.</summary>
public sealed record OfferSelectedEvent(
    Guid ReportId,
    Guid SelectedOfferId,
    Guid WinnerMechanicId,
    IReadOnlyCollection<Guid> OtherMechanicIds,
    Guid CustomerId,
    DateTimeOffset OccurredAt) : IDomainEvent;
