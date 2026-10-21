using System.Threading.Channels;
using IndustryApp.Application.Abstractions;
using IndustryApp.Domain.Common;

namespace IndustryApp.Infrastructure.Events;

// [UC-03][PAT-03.1] Observer / Publish–Subscribe
// Niyet   : Rapor oluşturmanın eşleştirmeyi, eşleştirmenin e-postayı bilmemesi; modüllerin olayla haberleşmesi.
// Neden   : Doğrudan servis çağrıları modülleri birbirine zincirler ve HTTP yanıtını eşleştirme/e-posta süresine bağlardı.
//           MediatR yerine kendi kodumuz: desen projenin kendi sınıflarında görünür.
// Kazanım : Yeni tepki yeni bir IEventHandler<T>'dir, yayınlayan değişmez; yanıt süresi arka plan işinden bağımsızdır.
// Metrik  : Rapor oluşturma p95 < 500 ms, eşleştirmeden bağımsız (CreateReport_ReturnsBeforeMatchingCompletes, TC-02.3.2).
/// <summary>
/// Olayları bellekteki System.Threading.Channels kuyruğuna yazar ve hemen döner. Singleton'dır (PAT-X.2).
/// Geçici çözüm: süreç kapanırsa işlenmemiş olaylar kaybolur (T-9.1); kalıcı çözüm Transactional Outbox.
/// </summary>
public sealed class InProcessEventDispatcher : IEventDispatcher
{
    private readonly Channel<IDomainEvent> _channel = Channel.CreateUnbounded<IDomainEvent>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public ChannelReader<IDomainEvent> Reader => _channel.Reader;

    public async ValueTask EnqueueAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct)
    {
        foreach (var domainEvent in events)
            await _channel.Writer.WriteAsync(domainEvent, ct);
    }
}
