namespace IndustryApp.Domain.Common;

/// <summary>
/// Domain ve Application katmanlarının ortak kullandığı hata kodları (T-11.4).
/// Frontend kullanıcıya gösterilecek Türkçe mesajı bu koda göre seçer.
/// </summary>
public static class ErrorCodes
{
    // Genel
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string UniqueViolation = "UNIQUE_VIOLATION";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string Unexpected = "UNEXPECTED_ERROR";
    public const string UnknownRole = "UNKNOWN_ROLE";

    // Hesaplar (UC-01, YA-01)
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string EmailAlreadyUsed = "EMAIL_ALREADY_USED";
    public const string TaxNumberAlreadyUsed = "TAX_NUMBER_ALREADY_USED";
    public const string ConsentRequired = "CONSENT_REQUIRED";
    public const string MechanicNotApproved = "MECHANIC_NOT_APPROVED";
    public const string MechanicInvalidStatus = "MECHANIC_INVALID_STATUS";
    public const string MechanicSpecialtyRequired = "MECHANIC_SPECIALTY_REQUIRED";
    public const string RejectionReasonRequired = "REJECTION_REASON_REQUIRED";
    public const string InvalidLocation = "INVALID_LOCATION";

    // Katalog
    public const string CategoryMaxDepth = "CATEGORY_MAX_DEPTH";
    public const string CategoryInactive = "CATEGORY_INACTIVE";

    // Raporlar (UC-02)
    public const string ReportNotFound = "REPORT_NOT_FOUND";
    public const string ReportIncomplete = "REPORT_INCOMPLETE";
    public const string ReportInvalidState = "REPORT_INVALID_STATE";
    public const string NotReportOwner = "NOT_REPORT_OWNER";
    public const string OpenReportLimitReached = "OPEN_REPORT_LIMIT_REACHED";
    public const string DuplicateDamageItem = "DUPLICATE_DAMAGE_ITEM";

    // Eşleştirme ve teklifler (UC-03)
    public const string NotMatched = "NOT_MATCHED";
    public const string OfferNotFound = "OFFER_NOT_FOUND";
    public const string OfferAlreadyExists = "OFFER_ALREADY_EXISTS";
    public const string OfferInvalidState = "OFFER_INVALID_STATE";
    public const string OfferLaborRequired = "OFFER_LABOR_REQUIRED";
    public const string OfferTotalInvalid = "OFFER_TOTAL_INVALID";
    public const string OfferEstimatedDaysInvalid = "OFFER_ESTIMATED_DAYS_INVALID";
    public const string PriceAmountInvalid = "PRICE_AMOUNT_INVALID";

    // Seçim ve iletişim (UC-04)
    public const string OfferNotAvailable = "OFFER_NOT_AVAILABLE";
    public const string UndoWindowExpired = "UNDO_WINDOW_EXPIRED";
}
