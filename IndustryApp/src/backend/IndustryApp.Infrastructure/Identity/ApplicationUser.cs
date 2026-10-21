using Microsoft.AspNetCore.Identity;

namespace IndustryApp.Infrastructure.Identity;

/// <summary>
/// Kullanıcı hesabı (e-posta, şifre özeti, telefon). Identity'ye aittir; Domain kullanıcıyı yalnızca
/// Guid UserId olarak tanır. Telefon her iki rol için ortak olduğundan burada, tek kaynaktadır (T-6.3).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = default!;

    /// <summary>Kullanıcı silinmez; pasifleştirilir.</summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
}
