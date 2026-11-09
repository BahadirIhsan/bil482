using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Common.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IndustryApp.Infrastructure.Identity;

/// <summary>
/// Access token (HS256 JWT) ve refresh token üretir (K-11). Claim'ler: sub, email, name, role (bir veya daha
/// fazla), mechanic_status (yalnızca ustada). Refresh token 64 baytlık rastgele değerdir; kullanıcıya düz hali,
/// veritabanına SHA-256 özeti gider. Süreler Jwt ayarlarından okunur.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider clock) : ITokenService
{
    public const string MechanicStatusClaim = "mechanic_status";

    private readonly JwtOptions _jwt = options.Value;

    public IAccessToken CreateAccessToken(IUserTokenClaims claims)
    {
        var now = clock.UtcNow;
        var expires = now.AddMinutes(_jwt.AccessTokenMinutes);

        var list = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, claims.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, claims.Email),
            new(JwtRegisteredClaimNames.Name, claims.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        list.AddRange(claims.Roles.Select(role => new Claim("role", role)));
        if (claims.MechanicStatus is not null)
            list.Add(new Claim(MechanicStatusClaim, claims.MechanicStatus));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _jwt.Issuer, _jwt.Audience, list, now.UtcDateTime, expires.UtcDateTime, credentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public IRefreshTokenPair CreateRefreshToken()
    {
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        return new RefreshTokenPair(token, HashRefreshToken(token), clock.UtcNow.AddDays(_jwt.RefreshTokenDays));
    }

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    private sealed record AccessToken(string Token, DateTimeOffset ExpiresAt) : IAccessToken;

    private sealed record RefreshTokenPair(string Token, string TokenHash, DateTimeOffset ExpiresAt) : IRefreshTokenPair;
}
