namespace IndustryApp.Application.Notifications;

/// <summary>SignalR olay adları (T-12). Frontend useRealtimeEvent ile aynı adları dinler.</summary>
public static class RealtimeEvents
{
    public const string NotificationCreated = "notification.created";
    public const string OfferCreated = "offer.created";
    public const string OfferUpdated = "offer.updated";
    public const string OfferWithdrawn = "offer.withdrawn";
    public const string ReportStatusChanged = "report.statusChanged";
}
