using Microsoft.AspNetCore.SignalR;

namespace IndustryApp.WebApi.Realtime;

/// <summary>SignalR'ın Clients.User(userId) adresi: JWT'deki sub claim'i (kullanıcı Guid'i).</summary>
public sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) => connection.User?.FindFirst("sub")?.Value;
}
