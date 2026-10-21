namespace IndustryApp.Application.Abstractions;

/// <summary>
/// Gerçek zamanlı yayın portu. Application SignalR'ı bilmez; implementasyon WebApi'dedir.
/// Olay adları: notification.created, offer.created, offer.updated, offer.withdrawn, report.statusChanged.
/// </summary>
public interface IRealtimePublisher
{
    Task PublishToUserAsync(Guid userId, string eventName, object payload, CancellationToken ct);
}
