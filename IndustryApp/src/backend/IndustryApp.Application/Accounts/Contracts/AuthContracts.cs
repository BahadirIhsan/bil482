namespace IndustryApp.Application.Accounts.Contracts;

/// <summary>Refresh ve logout istekleri (endpoint 4, 5).</summary>
public interface IRefreshTokenRequest
{
    string RefreshToken { get; }
}

/// <summary>Giriş ve yenileme yanıtı. Access token bellekte, refresh token istemcide saklanır (T-14.6).</summary>
public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthUserResponse User);

/// <summary>Oturumdaki kullanıcı; frontend role ve usta onay durumuna göre yönlendirir (T-14.3).</summary>
public sealed record AuthUserResponse(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles,
    string? MechanicStatus);

/// <summary>GET /api/account/me (endpoint 6).</summary>
public sealed record MeResponse(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    IReadOnlyCollection<string> Roles,
    string? MechanicStatus);

/// <summary>IUserTokenClaims'in Application içindeki taşıyıcısı.</summary>
public sealed record UserTokenClaims(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    string? MechanicStatus) : Abstractions.IUserTokenClaims;
