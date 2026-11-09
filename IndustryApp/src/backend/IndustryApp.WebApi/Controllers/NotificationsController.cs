using IndustryApp.Application.Common;
using IndustryApp.Application.Notifications;
using IndustryApp.Application.Notifications.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>Kullanıcının bildirim kutusu (endpoint 33–36). Yeni bildirimler ayrıca notification.created ile gelir.</summary>
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(INotificationService notifications) : ApiControllerBase
{
    /// <summary>33 · En yeni önce, sayfalı (pageSize en fazla 50).</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<NotificationResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        ToActionResult(await notifications.ListMineAsync(new PagedQuery(page, pageSize), ct));

    /// <summary>34 · Okunmamış bildirim sayısı.</summary>
    [HttpGet("unread-count")]
    [ProducesResponseType<UnreadCountResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UnreadCount(CancellationToken ct) =>
        Ok(new UnreadCountResponse(await notifications.GetUnreadCountAsync(ct)));

    /// <summary>35 · Bir bildirimi okundu yapar; başkasının bildirimi 404 döner.</summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct) =>
        ToActionResult(await notifications.MarkReadAsync(id, ct));

    /// <summary>36 · Tüm bildirimleri okundu yapar.</summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct) =>
        ToActionResult(await notifications.MarkAllReadAsync(ct));

    private sealed record PagedQuery(int Page, int PageSize) : IPagedQuery;
}
