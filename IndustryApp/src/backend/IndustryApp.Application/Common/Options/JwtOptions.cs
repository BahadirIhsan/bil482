using System.ComponentModel.DataAnnotations;

namespace IndustryApp.Application.Common.Options;

/// <summary>"Jwt" bölümü. SigningKey yerelde user-secrets'ta, üretimde Jwt__SigningKey ortam değişkenindedir.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = default!;

    [Required]
    public string Audience { get; init; } = default!;

    [Required]
    public string SigningKey { get; init; } = default!;

    [Range(1, 24 * 60)]
    public int AccessTokenMinutes { get; init; }

    [Range(1, 365)]
    public int RefreshTokenDays { get; init; }
}
