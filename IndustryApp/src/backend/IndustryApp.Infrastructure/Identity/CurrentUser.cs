using System.Security.Claims;
using IndustryApp.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace IndustryApp.Infrastructure.Identity;

/// <summary>
/// İsteği yapan kullanıcı: JWT'deki sub claim'i. HTTP isteği dışında (arka plan olay işleme) HttpContext yoktur;
/// o zaman sistem kullanıcısı gibi davranır: UserId = Guid.Empty, kimliksiz.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid UserId =>
        Guid.TryParse(Principal?.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true && UserId != Guid.Empty;

    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;

    public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
