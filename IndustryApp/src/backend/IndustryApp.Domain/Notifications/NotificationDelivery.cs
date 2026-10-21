using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Notifications;

/// <summary>
/// Bir bildirimin bir kanaldan gönderim sonucu (notification_deliveries). Circuit Breaker açıkken
/// atlanan e-postalar sessizce kaybolmaz, burada CircuitOpen olarak görünür.
/// </summary>
public sealed class NotificationDelivery : BaseEntity<long>
{
    /// <summary>last_error kolonunun uzunluğu (T-6.6).</summary>
    public const int MaxErrorLength = 500;

    private NotificationDelivery()
    {
    }

    public Guid NotificationId { get; private set; }

    public NotificationChannelType Channel { get; private set; }

    public NotificationDeliveryStatus Status { get; private set; }

    public short AttemptCount { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static NotificationDelivery Create(
        Guid notificationId,
        NotificationChannelType channel,
        NotificationDeliveryStatus status,
        int attemptCount,
        string? lastError,
        DateTimeOffset now) =>
        new()
        {
            NotificationId = notificationId,
            Channel = channel,
            Status = status,
            AttemptCount = (short)Math.Max(1, attemptCount),
            LastError = lastError is { Length: > MaxErrorLength } ? lastError[..MaxErrorLength] : lastError,
            CreatedAt = now
        };
}
