using IndustryApp.Application.Accounts;
using IndustryApp.Application.Accounts.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>Oturumdaki kullanıcının hesabı (endpoint 6). Profil güncelleme ve rol paneli sonraki sürümlerde.</summary>
[Route("api/account")]
[Authorize]
public sealed class AccountController(IAccountService accounts) : ApiControllerBase
{
    /// <summary>6 · Ad, e-posta, telefon, roller ve usta onay durumu.</summary>
    [HttpGet("me")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken ct) =>
        ToActionResult(await accounts.GetMeAsync(ct));
}
