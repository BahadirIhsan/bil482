namespace IndustryApp.Domain.Notifications;

/// <summary>Kanal başına gönderim sonucu. CircuitOpen, Circuit Breaker'ın etkisini görünür kılar (PAT-03.6).</summary>
public enum NotificationDeliveryStatus
{
    Sent,
    Failed,
    Skipped,
    CircuitOpen
}
