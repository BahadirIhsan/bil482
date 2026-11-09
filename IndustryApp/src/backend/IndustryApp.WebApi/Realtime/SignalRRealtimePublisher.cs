using IndustryApp.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace IndustryApp.WebApi.Realtime;

/// <summary>
/// IRealtimePublisher'ın SignalR uygulaması. Application SignalR'ı bilmez; olay handler'ları bu port üzerinden
/// yayın yapar. Kullanıcı çevrimdışıysa mesaj düşer; kalıcı kayıt notifications tablosundadır.
/// </summary>
public sealed class SignalRRealtimePublisher(IHubContext<NotificationHub> hub) : IRealtimePublisher
{
    public Task PublishToUserAsync(Guid userId, string eventName, object payload, CancellationToken ct) =>
        hub.Clients.User(userId.ToString()).SendAsync(eventName, payload, ct);
}
