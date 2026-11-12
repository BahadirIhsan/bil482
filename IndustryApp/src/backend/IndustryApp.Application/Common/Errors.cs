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
        public static readonly Error InvalidSpecialty = Error.Validation(ErrorCodes.InvalidSpecialty, "Uzmanlıklar geçerli ana hasar kategorilerinden seçilmelidir.");
        public static readonly Error AccountLocked = Error.Unauthorized(ErrorCodes.AccountLocked, "Çok fazla hatalı deneme yapıldı. Lütfen 15 dakika sonra tekrar deneyin.");
    }

    public static class Reports
    {
        public static readonly Error NotFound = Error.NotFound(ErrorCodes.ReportNotFound, "Rapor bulunamadı.");
        public static readonly Error NotOwner = Error.Forbidden(ErrorCodes.NotReportOwner, "Bu rapor size ait değil.");
        public static readonly Error OpenLimitReached = Error.BusinessRule(ErrorCodes.OpenReportLimitReached, "Açık rapor sınırına ulaştınız.");
        public static readonly Error InvalidPhotoKey = Error.Validation(ErrorCodes.InvalidPhotoKey, "Fotoğraf bulunamadı veya size ait değil. Fotoğrafı yeniden yükleyin.");
        public static readonly Error VehicleTypeInvalid = Error.Validation(ErrorCodes.ValidationFailed, "Geçerli bir araç türü seçin.");
        public static readonly Error CategoryInactive = Error.Validation(ErrorCodes.CategoryInactive, "Seçilen hasar bölgelerinden biri artık kullanılmıyor.");
    }

    public static class Photos
    {
        public static readonly Error NotFound = Error.NotFound(ErrorCodes.PhotoNotFound, "Fotoğraf bulunamadı.");
        public static readonly Error UnsupportedImage = Error.Validation(ErrorCodes.UnsupportedImage, "Yalnızca JPEG, PNG veya WebP fotoğraf yüklenebilir.");

        public static Error FileTooLarge(int maxMb) =>
            Error.Validation(ErrorCodes.FileTooLarge, $"Fotoğraf en fazla {maxMb} MB olabilir.");
    }

    public static class Offers
    {
        public static readonly Error NotFound = Error.NotFound(ErrorCodes.OfferNotFound, "Teklif bulunamadı.");
        public static readonly Error NotMatched = Error.Forbidden(ErrorCodes.NotMatched, "Bu raporla eşleşmeniz yok.");
        public static readonly Error AlreadyExists = Error.Conflict(ErrorCodes.OfferAlreadyExists, "Bu rapora zaten teklif verdiniz.");
        public static readonly Error NotAvailable = Error.Conflict(ErrorCodes.OfferNotAvailable, "Teklif seçilebilir durumda değil.");
        public static readonly Error UndoWindowExpired = Error.Conflict(ErrorCodes.UndoWindowExpired, "Seçimi geri alma süresi doldu.");
        public static readonly Error OfferIdRequired = Error.Validation(ErrorCodes.ValidationFailed, "Seçilecek teklif belirtilmelidir.");
    }

    public static class Selection
    {
        public static readonly Error NothingToUndo = Error.Conflict(ErrorCodes.ReportInvalidState, "Raporda geri alınabilecek bir seçim yok.");
        public static readonly Error NoActiveSelection = Error.Conflict(ErrorCodes.NoActiveSelection, "Bu raporda henüz bir teklif seçilmedi.");
    }

    public static class Contacts
    {
        public static readonly Error NotFound = Error.NotFound(ErrorCodes.ContactNotFound, "İletişim bilgisi bulunamadı.");
    }

    public static class Notifications
    {
        public static readonly Error NotFound = Error.NotFound(ErrorCodes.NotificationNotFound, "Bildirim bulunamadı.");
    }
}
