using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public async Task AddAsync(Guid userId, string tokenHash, DateTimeOffset expiresAt, DateTimeOffset now, string? createdByIp, CancellationToken ct) =>
        await db.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            CreatedByIp = createdByIp
        }, ct);

    public async Task<IStoredRefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct) =>
        await db.RefreshTokens.AsNoTracking()
            .Where(t => t.TokenHash == tokenHash)
            .Select(t => new StoredRefreshToken(t.Id, t.UserId, t.ExpiresAt, t.RevokedAt))
            .FirstOrDefaultAsync(ct);

    public async Task RevokeAsync(Guid tokenId, DateTimeOffset now, string? replacedByTokenHash, CancellationToken ct)
    {
        var token = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Id == tokenId, ct);
        if (token is null || token.RevokedAt is not null)
            return;

        token.RevokedAt = now;
        token.ReplacedByTokenHash = replacedByTokenHash;
    }

    public async Task<int> RevokeAllActiveAsync(Guid userId, DateTimeOffset now, CancellationToken ct)
    {
        var active = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(ct);
        active.ForEach(t => t.RevokedAt = now);
        return active.Count;
    }

    private sealed record StoredRefreshToken(Guid Id, Guid UserId, DateTimeOffset ExpiresAt, DateTimeOffset? RevokedAt) : IStoredRefreshToken;
}
