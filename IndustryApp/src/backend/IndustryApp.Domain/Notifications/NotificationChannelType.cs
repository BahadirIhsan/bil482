namespace IndustryApp.Domain.Notifications;

/// <summary>Bildirim kanalı (K-09). Sms için arayüz hazır, sağlayıcı yok.</summary>
public enum NotificationChannelType
{
    InApp,
    Email,
    Sms
}
