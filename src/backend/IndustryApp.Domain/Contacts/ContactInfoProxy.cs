using IndustryApp.Domain.Reports;

namespace IndustryApp.Domain.Contacts;

// [UC-04][PAT-04.3] Proxy (koruma)
// Niyet   : Telefon ve e-postanın gösterilip gösterilmeyeceğine tek bir yerde karar vermek.
// Neden   : Her ekran ayrı kontrol yapsaydı biri unutulabilirdi; kişisel veri sızıntısı KVKK ihlalidir.
// Kazanım : Erişim kontrolü merkezi ve birim test edilebilir; yeni ekranlar korumayı otomatik alır.
// Metrik  : Yetkisiz yanıtlarda gerçek numara sayısı 0 (ContactProxy_NotMatched_ReturnsMaskedPhone, TC-04.4.1).
public sealed class ContactInfoProxy(IContactInfo real, AccidentReport report, Guid viewerId) : IContactInfo
{
    public bool IsRevealed => report.IsMatchedWith(viewerId);

    public string FullName => real.FullName;

    public string Phone => IsRevealed ? real.Phone : Mask(real.Phone);

    public string Email => IsRevealed ? real.Email : "-";

    private static string Mask(string? phone) =>
        string.IsNullOrWhiteSpace(phone) || phone.Length < 2 ? "-" : $"05** *** ** {phone[^2..]}";
}
