using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public async Task AddAsync(Notification notification, CancellationToken ct) =>
        await db.Notifications.AddAsync(notification, ct);

    public Task<PagedResult<NotificationResponse>> ListByUserAsync(Guid userId, IPagedQuery paging, CancellationToken ct) =>
        db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponse(n.Id, n.Type, n.Title, n.Body, n.Payload, n.IsRead, n.CreatedAt, n.ReadAt))
            .ToPagedResultAsync(paging, ct);

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken ct) =>
        db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task<bool> MarkReadAsync(Guid userId, Guid notificationId, DateTimeOffset now, CancellationToken ct)
    {
        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);
        if (notification is null)
            return false;

        notification.MarkRead(now);
        return true;
    }

    public async Task<int> MarkAllReadAsync(Guid userId, DateTimeOffset now, CancellationToken ct)
    {
        var unread = await db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync(ct);
        unread.ForEach(n => n.MarkRead(now));
        return unread.Count;
    }

    public async Task AddDeliveryAsync(NotificationDelivery delivery, CancellationToken ct) =>
        await db.NotificationDeliveries.AddAsync(delivery, ct);
}
