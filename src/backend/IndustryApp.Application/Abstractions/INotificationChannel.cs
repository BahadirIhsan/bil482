using IndustryApp.Domain.Notifications;

namespace IndustryApp.Application.Abstractions;

/// <summary>Bildirim kanalı adaptörü (PAT-03.5). E-posta ve SMS aynı arayüzü uygular.</summary>
public interface INotificationChannel
{
    NotificationChannelType Channel { get; }

    Task<IDeliveryResult> SendAsync(INotificationMessage message, CancellationToken ct);
}

/// <summary>Kanala gönderilecek mesaj.</summary>
public interface INotificationMessage
{
    Guid NotificationId { get; }

    Guid UserId { get; }

    string Type { get; }

    string Title { get; }

    string Body { get; }

    /// <summary>Bildirime tıklanınca açılacak uygulama içi bağlantı; yoksa null.</summary>
    string? Link { get; }

    string? RecipientEmail { get; }

    string? RecipientPhone { get; }
}

/// <summary>Kanal gönderim sonucu; notification_deliveries tablosuna yazılır.</summary>
public interface IDeliveryResult
{
    NotificationDeliveryStatus Status { get; }

    int AttemptCount { get; }

    string? Error { get; }
}
