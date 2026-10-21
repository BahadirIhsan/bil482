namespace IndustryApp.Domain.Common;

/// <summary>
/// Tüm entity'lerin tabanı. Olay listesi burada tutulur; UnitOfWork kayıttan sonra bu listeyi okur.
/// </summary>
public abstract class BaseEntity<TId> : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public TId Id { get; protected set; } = default!;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
