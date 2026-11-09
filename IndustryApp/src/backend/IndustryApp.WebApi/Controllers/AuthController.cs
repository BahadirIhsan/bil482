using IndustryApp.Application.Accounts;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.WebApi.Contracts;
using IndustryApp.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IndustryApp.WebApi.Controllers;

/// <summary>
/// UC-01 kimlik uç noktaları (endpoint 1–5). İki kayıt endpoint'i farklı gövde alır ama aynı RegisterAsync'i
/// çağırır; hangi profilin üretileceğine IUserProfileFactory karar verir (PAT-01.1).
/// </summary>
[Route("api/auth")]
public sealed class AuthController(IAuthService auth) : ApiControllerBase
{
    /// <summary>1 · Müşteri kaydı.</summary>
    [HttpPost("register/customer")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.RegisterPolicy)]
    [ProducesResponseType<RegistrationResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterCustomer(RegisterCustomerRequest request, CancellationToken ct) =>
        ToActionResult(await auth.RegisterAsync(request, ct));

    /// <summary>2 · Usta kaydı (admin onayı bekler).</summary>
    [HttpPost("register/mechanic")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.RegisterPolicy)]
    [ProducesResponseType<RegistrationResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterMechanic(RegisterMechanicRequest request, CancellationToken ct) =>
        ToActionResult(await auth.RegisterAsync(request, ct));

    /// <summary>3 · Giriş; access ve refresh token döner.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct) =>
        ToActionResult(await auth.LoginAsync(request, ct));

    /// <summary>4 · Refresh token ile yeni token çifti (rotation).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken ct) =>
        ToActionResult(await auth.RefreshAsync(request, ct));

    /// <summary>5 · Çıkış; refresh token kapatılır.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct) =>
        ToActionResult(await auth.LogoutAsync(request, ct));
}
