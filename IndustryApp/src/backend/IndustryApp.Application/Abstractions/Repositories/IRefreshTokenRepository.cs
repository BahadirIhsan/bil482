namespace IndustryApp.Application.Abstractions.Repositories;

/// <summary>Refresh token kayıtları (refresh_tokens). Yalnızca SHA-256 özeti saklanır (NFR-S2).</summary>
public interface IRefreshTokenRepository
{
    Task AddAsync(Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset now, string? createdByIp, CancellationToken ct);

    Task<IStoredRefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct);

    /// <summary>Token'ı kapatır; rotasyonda yerine gelen token'ın özeti yazılır.</summary>
    Task RevokeAsync(Guid tokenId, DateTimeOffset now, string? replacedByTokenHash, CancellationToken ct);

    /// <summary>Kullanıcının tüm aktif token'larını kapatır (çalıntı token belirtisi). Kapatılan sayıyı döner.</summary>
    Task<int> RevokeAllActiveAsync(Guid userId, DateTimeOffset now, CancellationToken ct);
}

/// <summary>Kayıtlı refresh token'ın okunan alanları.</summary>
public interface IStoredRefreshToken
{
    Guid Id { get; }

    Guid UserId { get; }

    DateTimeOffset ExpiresAt { get; }

    DateTimeOffset? RevokedAt { get; }
}
