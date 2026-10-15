using IndustryApp.Domain.Common;

namespace IndustryApp.Application.Abstractions;

/// <summary>
/// Bir olay türünün dinleyicisi (PAT-03.1). Yeni tepki = yeni handler; yayınlayan kod değişmez.
/// DI'a Scrutor taramasıyla otomatik kaydedilir.
/// </summary>
public interface IEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct);
}
