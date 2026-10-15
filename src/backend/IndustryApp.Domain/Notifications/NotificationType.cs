namespace IndustryApp.Domain.Notifications;

/// <summary>Bildirim türleri (notifications.type). Frontend bunları Türkçe etikete çevirir.</summary>
public static class NotificationType
{
    public const string NewJob = "NEW_JOB";
    public const string NoMechanicFound = "NO_MECHANIC_FOUND";
    public const string NewOffer = "NEW_OFFER";
    public const string OfferSelected = "OFFER_SELECTED";
    public const string OfferNotSelected = "OFFER_NOT_SELECTED";
    public const string SelectionUndone = "SELECTION_UNDONE";
    public const string JobCompleted = "JOB_COMPLETED";
    public const string ReportCancelled = "REPORT_CANCELLED";
    public const string ReportExpired = "REPORT_EXPIRED";
    public const string MechanicApproved = "MECHANIC_APPROVED";
    public const string MechanicRejected = "MECHANIC_REJECTED";
}
