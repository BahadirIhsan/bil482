namespace IndustryApp.Application.Abstractions;

/// <summary>JWT access token ve refresh token üretimi (K-11).</summary>
public interface ITokenService
{
    IAccessToken CreateAccessToken(IUserTokenClaims claims);

    /// <summary>Düz token (kullanıcıya) + SHA-256 özeti (veritabanına).</summary>
    IRefreshTokenPair CreateRefreshToken();
}

public interface IAccessToken
{
    string Token { get; }

    DateTimeOffset ExpiresAt { get; }
}

public interface IRefreshTokenPair
{
    string Token { get; }

    string TokenHash { get; }

    DateTimeOffset ExpiresAt { get; }
}

/// <summary>Token'a yazılan claim'ler: sub, email, name, role, mechanic_status (yalnızca ustada).</summary>
public interface IUserTokenClaims
{
    Guid UserId { get; }

    string Email { get; }

    string FullName { get; }

    IReadOnlyCollection<string> Roles { get; }

    string? MechanicStatus { get; }
}
