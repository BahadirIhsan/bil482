namespace IndustryApp.Domain.Contacts;

/// <summary>İletişim bilgisinin ortak arayüzü; gerçek nesne ve koruma vekili aynı arayüzü uygular.</summary>
public interface IContactInfo
{
    string FullName { get; }

    string Phone { get; }

    string Email { get; }

    bool IsRevealed { get; }
}
