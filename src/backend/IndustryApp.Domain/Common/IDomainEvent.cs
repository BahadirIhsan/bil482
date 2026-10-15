namespace IndustryApp.Domain.Common;

/// <summary>Entity içinde üretilen ve kayıttan sonra yayınlanan domain olayı.</summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
