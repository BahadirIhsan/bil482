using IndustryApp.Domain.Common;

namespace IndustryApp.Application.Abstractions;

/// <summary>Kaydedilmiş olayları arka planda işlenmek üzere kuyruğa yazar; HTTP yanıtı işlenmeyi beklemez.</summary>
public interface IEventDispatcher
{
    ValueTask EnqueueAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct);
}
