namespace IndustryApp.Infrastructure.Identity;

/// <summary>
/// Refresh token kaydı. Token'ın kendisi değil yalnızca SHA-256 özeti saklanır (NFR-S2);
/// her kullanımda yenilenir (rotation) ve eskisi replaced_by_token_hash ile kapatılır.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = default!;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByIp { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
}
