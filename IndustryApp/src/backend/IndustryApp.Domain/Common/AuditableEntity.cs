namespace IndustryApp.Domain.Common;

/// <summary>
/// Oluşturulma ve güncellenme zamanını taşıyan entity tabanı.
/// Domain'de DateTime.UtcNow çağrılmaz; zaman her metoda parametre olarak gelir.
/// </summary>
public abstract class AuditableEntity<TId> : BaseEntity<TId>
{
    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset? UpdatedAt { get; protected set; }

    protected void Touch(DateTimeOffset now) => UpdatedAt = now;
}
