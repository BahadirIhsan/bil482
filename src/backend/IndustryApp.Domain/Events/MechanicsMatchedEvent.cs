using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Eşleştirme bitti (UC-03). Liste boşsa müşteriye NO_MECHANIC_FOUND gider.</summary>
public sealed record MechanicsMatchedEvent(
    Guid ReportId,
    Guid CustomerId,
    IReadOnlyCollection<Guid> MechanicIds,
    DateTimeOffset OccurredAt) : IDomainEvent;
