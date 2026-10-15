using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Admin ustayı gerekçesiyle reddetti (YA-01). Ustaya e-posta ve uygulama içi bildirim.</summary>
public sealed record MechanicRejectedEvent(
    Guid MechanicId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
