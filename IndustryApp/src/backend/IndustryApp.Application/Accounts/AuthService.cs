using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Accounts.Factories;
using IndustryApp.Application.Common;
using IndustryApp.Domain.Accounts;

namespace IndustryApp.Application.Accounts;

/// <summary>
/// UC-01 kimlik akışları. Kayıtta role bakılmaz: Role anahtarına uyan IUserProfileFactory seçilir (PAT-01.1).
/// Kullanıcı, profil ve sözleşme onayları tek transaction'dadır; profil adımı başarısızsa kullanıcı da geri alınır.
/// Refresh token her kullanımda yenilenir; iptal edilmiş bir token tekrar gelirse kullanıcının tüm token'ları kapanır.
/// </summary>
public sealed class AuthService(
    IEnumerable<IUserProfileFactory> factories,
    ILegalDocumentRepository legalDocuments,
    IUserConsentRepository consents,
    IIdentityService identity,
    ITokenService tokens,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider clock) : IAuthService
{
    public async Task<Result<RegistrationResponse>> RegisterAsync(IRegisterUserRequest request, CancellationToken ct)
    {
        var factory = factories.SingleOrDefault(f => f.Role == request.Role);
        if (factory is null)
            return Errors.Auth.UnknownRole;

        var documents = await legalDocuments.GetCurrentByCodesAsync(factory.RequiredLegalDocumentCodes, ct);
        var accepted = request.AcceptedLegalDocumentIds.ToHashSet();
        if (documents.Count != factory.RequiredLegalDocumentCodes.Count || !documents.All(d => accepted.Contains(d.Id)))
            return Errors.Auth.ConsentRequired;

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        var userId = await identity.CreateUserAsync(
            new CreateIdentityUserRequest(request.FullName, request.Email, request.Phone, request.Password, factory.Role), ct);
        if (userId.IsFailure)
            return userId.Error!;

        var profile = await factory.CreateProfileAsync(userId.Value, request, ct); // Factory Method
        if (profile.IsFailure)
            return profile.Error!; // transaction commit edilmeden kapanır → kullanıcı da geri alınır

        var now = clock.UtcNow;
        await consents.AddRangeAsync(documents.Select(d => UserConsent.Create(userId.Value, d.Id, now, currentUser.IpAddress)), ct);
        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new RegistrationResponse(userId.Value, factory.Role, RequiresApproval: factory.Role == RoleNames.Mechanic);
    }

    public async Task<Result<AuthResponse>> LoginAsync(ILoginRequest request, CancellationToken ct)
    {
        var claims = await identity.ValidateCredentialsAsync(request, ct);
        if (claims.IsFailure)
            return claims.Error!;

        return await IssueAsync(claims.Value, ct);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(IRefreshTokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Errors.Auth.TokenExpired;

        var now = clock.UtcNow;
        var stored = await refreshTokens.GetByHashAsync(tokens.HashRefreshToken(request.RefreshToken), ct);
        if (stored is null)
            return Errors.Auth.TokenExpired;

        if (stored.RevokedAt is not null)
        {
            // Kapatılmış token tekrar kullanıldı: çalıntı token belirtisi, kullanıcının tüm oturumları kapanır.
            await refreshTokens.RevokeAllActiveAsync(stored.UserId, now, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return Errors.Auth.TokenExpired;
        }

        if (stored.ExpiresAt <= now)
            return Errors.Auth.TokenExpired;

        var claims = await identity.GetClaimsAsync(stored.UserId, ct);
        if (claims is null)
            return Errors.Auth.TokenExpired; // kullanıcı pasifleştirilmiş

        return await IssueAsync(claims, ct, rotatedTokenId: stored.Id);
    }

    public async Task<Result> LogoutAsync(IRefreshTokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Success();

        var stored = await refreshTokens.GetByHashAsync(tokens.HashRefreshToken(request.RefreshToken), ct);
        if (stored is not null && stored.UserId == currentUser.UserId && stored.RevokedAt is null)
        {
            await refreshTokens.RevokeAsync(stored.Id, clock.UtcNow, null, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }

        // Çıkış her zaman başarılıdır; bilinmeyen token hakkında bilgi verilmez.
        return Result.Success();
    }

    private async Task<Result<AuthResponse>> IssueAsync(IUserTokenClaims claims, CancellationToken ct, Guid? rotatedTokenId = null)
    {
        var now = clock.UtcNow;
        var access = tokens.CreateAccessToken(claims);
        var refresh = tokens.CreateRefreshToken();

        if (rotatedTokenId is { } oldId)
            await refreshTokens.RevokeAsync(oldId, now, refresh.TokenHash, ct);
        await refreshTokens.AddAsync(claims.UserId, refresh.TokenHash, refresh.ExpiresAt, now, currentUser.IpAddress, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            access.Token,
            access.ExpiresAt,
            refresh.Token,
            refresh.ExpiresAt,
            new AuthUserResponse(claims.UserId, claims.FullName, claims.Email, claims.Roles, claims.MechanicStatus));
    }

    private sealed record CreateIdentityUserRequest(string FullName, string Email, string Phone, string Password, string Role)
        : ICreateIdentityUserRequest;
}
