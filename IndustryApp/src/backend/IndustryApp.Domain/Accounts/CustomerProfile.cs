using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Accounts;

/// <summary>Müşteri profili. Anahtarı kullanıcının kimliğidir (users 1–1 customer_profiles).</summary>
public sealed class CustomerProfile : AuditableEntity<Guid>
{
    private CustomerProfile()
    {
    }

    public Guid UserId => Id;

    public string City { get; private set; } = default!;

    public string? District { get; private set; }

    public static CustomerProfile Create(Guid userId, string city, string? district, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("İl boş olamaz.", nameof(city));

        return new CustomerProfile
        {
            Id = userId,
            City = city.Trim(),
            District = string.IsNullOrWhiteSpace(district) ? null : district.Trim(),
            CreatedAt = now
        };
    }

    public void Update(string city, string? district, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("İl boş olamaz.", nameof(city));

        City = city.Trim();
        District = string.IsNullOrWhiteSpace(district) ? null : district.Trim();
        Touch(now);
    }
}
