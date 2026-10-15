using IndustryApp.Domain.Common;

namespace IndustryApp.Application.Common;

/// <summary>Servislerin döndürdüğü hazır hatalar (T-11.4). Kodlar Domain'deki ErrorCodes ile aynıdır.</summary>
public static class Errors
{
    public static class Auth
    {
        public static readonly Error UnknownRole = Error.Validation(ErrorCodes.UnknownRole, "Geçersiz kullanıcı rolü.");
        public static readonly Error ConsentRequired = Error.Validation(ErrorCodes.ConsentRequired, "Zorunlu sözleşmeler onaylanmalıdır.");
        public static readonly Error InvalidCredentials = Error.Unauthorized(ErrorCodes.InvalidCredentials, "E-posta veya şifre hatalı.");
        public static readonly Error TokenExpired = Error.Unauthorized(ErrorCodes.TokenExpired, "Oturumun süresi doldu.");
        public static readonly Error EmailAlreadyUsed = Error.BusinessRule(ErrorCodes.EmailAlreadyUsed, "Bu e-posta adresi zaten kayıtlı.");
        public static readonly Error TaxNumberAlreadyUsed = Error.BusinessRule(ErrorCodes.TaxNumberAlreadyUsed, "Bu vergi numarası zaten kayıtlı.");
        public static readonly Error MechanicNotApproved = Error.Forbidden(ErrorCodes.MechanicNotApproved, "Hesabınız henüz onaylanmadı.");
    }

    public static class Reports
    {
        public static readonly Error NotFound = Error.NotFound(ErrorCodes.ReportNotFound, "Rapor bulunamadı.");
        public static readonly Error NotOwner = Error.Forbidden(ErrorCodes.NotReportOwner, "Bu rapor size ait değil.");
        public static readonly Error OpenLimitReached = Error.BusinessRule(ErrorCodes.OpenReportLimitReached, "Açık rapor sınırına ulaştınız.");
    }

    public static class Offers
    {
        public static readonly Error NotFound = Error.NotFound(ErrorCodes.OfferNotFound, "Teklif bulunamadı.");
        public static readonly Error NotMatched = Error.Forbidden(ErrorCodes.NotMatched, "Bu raporla eşleşmeniz yok.");
        public static readonly Error AlreadyExists = Error.Conflict(ErrorCodes.OfferAlreadyExists, "Bu rapora zaten teklif verdiniz.");
        public static readonly Error NotAvailable = Error.Conflict(ErrorCodes.OfferNotAvailable, "Teklif seçilebilir durumda değil.");
        public static readonly Error UndoWindowExpired = Error.Conflict(ErrorCodes.UndoWindowExpired, "Seçimi geri alma süresi doldu.");
    }
}
