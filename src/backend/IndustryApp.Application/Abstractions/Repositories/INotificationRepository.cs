using IndustryApp.Application.Common;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Domain.Notifications;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken ct);

    Task<PagedResult<NotificationResponse>> ListByUserAsync(Guid userId, IPagedQuery paging, CancellationToken ct);

    Task<int> CountUnreadAsync(Guid userId, CancellationToken ct);

    /// <summary>Bildirim kullanıcıya ait değilse veya yoksa false.</summary>
    Task<bool> MarkReadAsync(Guid userId, Guid notificationId, DateTimeOffset now, CancellationToken ct);

    /// <summary>Okundu yapılan bildirim sayısını döner.</summary>
    Task<int> MarkAllReadAsync(Guid userId, DateTimeOffset now, CancellationToken ct);

    Task AddDeliveryAsync(NotificationDelivery delivery, CancellationToken ct);
}
