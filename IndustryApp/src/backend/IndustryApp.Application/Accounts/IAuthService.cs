using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Common;

namespace IndustryApp.Application.Accounts;

/// <summary>Kayıt, giriş, token yenileme ve çıkış (UC-01, endpoint 1–5).</summary>
public interface IAuthService
{
    Task<Result<RegistrationResponse>> RegisterAsync(IRegisterUserRequest request, CancellationToken ct);

    Task<Result<AuthResponse>> LoginAsync(ILoginRequest request, CancellationToken ct);

    Task<Result<AuthResponse>> RefreshAsync(IRefreshTokenRequest request, CancellationToken ct);

    Task<Result> LogoutAsync(IRefreshTokenRequest request, CancellationToken ct);
}
