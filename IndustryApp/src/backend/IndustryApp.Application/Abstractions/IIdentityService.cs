using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Common;

namespace IndustryApp.Application.Abstractions;

/// <summary>
/// Identity kütüphanesini bir port arkasına alır. UserManager Infrastructure'da kalır; harici kimlik
/// sağlayıcısına geçmek yalnızca Infrastructure'ı etkiler.
/// </summary>
public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(ICreateIdentityUserRequest request, CancellationToken ct);

    Task<Result<IUserTokenClaims>> ValidateCredentialsAsync(ILoginRequest request, CancellationToken ct);

    Task<IUserTokenClaims?> GetClaimsAsync(Guid userId, CancellationToken ct);

    Task<IUserContact?> GetContactAsync(Guid userId, CancellationToken ct);
}

public interface ICreateIdentityUserRequest
{
    string FullName { get; }

    string Email { get; }

    string Phone { get; }

    string Password { get; }

    string Role { get; }
}

/// <summary>Kullanıcının gerçek iletişim bilgisi; yalnızca ContactInfoProxy ile sarılarak dışarı verilir.</summary>
public interface IUserContact
{
    Guid UserId { get; }

    string FullName { get; }

    string Phone { get; }

    string Email { get; }
}
