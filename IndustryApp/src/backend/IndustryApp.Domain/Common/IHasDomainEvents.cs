namespace IndustryApp.Domain.Common;

/// <summary>UnitOfWork, kayıttan sonra olayları bu arayüz üzerinden toplar.</summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
