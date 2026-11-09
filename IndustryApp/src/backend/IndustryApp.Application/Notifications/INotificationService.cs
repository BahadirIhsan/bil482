using IndustryApp.Application.Common;
using IndustryApp.Application.Notifications.Contracts;

namespace IndustryApp.Application.Notifications;

/// <summary>Bildirim hattı (T-9.3) ve kullanıcının bildirim kutusu (endpoint 33–36).</summary>
public interface INotificationService
{
    /// <summary>Yalnızca olay handler'ları çağırır: kaydet → notification.created → ek kanallar → teslim kayıtları.</summary>
    Task NotifyAsync(INotifyRequest request, CancellationToken ct);

    Task<Result<PagedResult<NotificationResponse>>> ListMineAsync(IPagedQuery query, CancellationToken ct);

    Task<int> GetUnreadCountAsync(CancellationToken ct);

    Task<Result> MarkReadAsync(Guid notificationId, CancellationToken ct);

    Task<Result> MarkAllReadAsync(CancellationToken ct);
}
