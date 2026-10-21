using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Accounts;

/// <summary>Kullanıcının bir sözleşme sürümünü onayladığı kayıt (FR-01.2, KVKK).</summary>
public sealed class UserConsent : BaseEntity<long>
{
    private UserConsent()
    {
    }

    public Guid UserId { get; private set; }

    public int LegalDocumentId { get; private set; }

    public DateTimeOffset AcceptedAt { get; private set; }

    public string? IpAddress { get; private set; }

    public static UserConsent Create(Guid userId, int legalDocumentId, DateTimeOffset now, string? ipAddress) =>
        new()
        {
            UserId = userId,
            LegalDocumentId = legalDocumentId,
            AcceptedAt = now,
            IpAddress = ipAddress
        };
}
