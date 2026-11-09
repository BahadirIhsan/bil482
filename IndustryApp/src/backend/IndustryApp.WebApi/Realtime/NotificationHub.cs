using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IndustryApp.WebApi.Realtime;

/// <summary>
/// /hubs/notifications. Yalnızca sunucudan istemciye yayın yapar; istemci hub'a işlem göndermez, her işlem
/// REST'ten geçer (T-12). Bağlantı kullanıcı kimliğiyle (sub) eşlenir.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
    public const string Path = "/hubs/notifications";
}
