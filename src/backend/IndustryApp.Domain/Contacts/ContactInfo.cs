namespace IndustryApp.Domain.Contacts;

/// <summary>Gerçek iletişim bilgisi. Dışarıya yalnızca ContactInfoProxy ile sarılarak verilir.</summary>
public sealed class ContactInfo(string fullName, string phone, string email) : IContactInfo
{
    public string FullName { get; } = fullName;

    public string Phone { get; } = phone;

    public string Email { get; } = email;

    public bool IsRevealed => true;
}
