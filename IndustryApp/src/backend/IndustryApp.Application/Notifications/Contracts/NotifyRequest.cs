using IndustryApp.Domain.Notifications;

namespace IndustryApp.Application.Notifications.Contracts;

/// <summary>Bir kullanıcıya bildirim isteği. Yalnızca olay handler'ları oluşturur (T-9.3).</summary>
public interface INotifyRequest
{
    Guid UserId { get; }

    string Type { get; }

    string Title { get; }

    string Body { get; }

    /// <summary>Tıklanınca ilgili ekrana gitmek için JSON'a çevrilecek nesne; ör. new { reportId }.</summary>
    object? Payload { get; }

    /// <summary>Uygulama içi kanal her zaman vardır; burada ek kanallar (Email, Sms) istenir.</summary>
    IReadOnlyCollection<NotificationChannelType> Channels { get; }
}

public sealed record NotifyRequest(
    Guid UserId,
    string Type,
    string Title,
    string Body,
    object? Payload = null,
    IReadOnlyCollection<NotificationChannelType>? ExtraChannels = null) : INotifyRequest
{
    public IReadOnlyCollection<NotificationChannelType> Channels => ExtraChannels ?? [];
}

/// <summary>notification.created SignalR yükü (T-12); liste satırıyla aynı alanlar.</summary>
public sealed record NotificationCreatedPayload(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? Payload,
    DateTimeOffset CreatedAt);

/// <summary>GET /api/notifications/unread-count yanıtı (endpoint 34).</summary>
public sealed record UnreadCountResponse(int Count);
