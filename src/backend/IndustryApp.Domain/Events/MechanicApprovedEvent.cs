using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Events;

/// <summary>Admin ustayı onayladı (YA-01). Ustaya e-posta ve uygulama içi bildirim.</summary>
public sealed record MechanicApprovedEvent(
    Guid MechanicId,
    DateTimeOffset OccurredAt) : IDomainEvent;
