using System.Text.Json;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace IndustryApp.Application.Notifications;

/// <summary>
/// T-9.3 bildirim hattı: 1) notifications kaydı (kullanıcı çevrimdışıyken de kaybolmaz), 2) notification.created
/// anlık yayını, 3) istenen ek kanallar, 4) her kanalın sonucu notification_deliveries'e. E-posta kanalı v0.3'te
/// gelir; o zamana kadar istenirse Skipped yazılır. Telefon ve e-posta asla loglanmaz.
/// </summary>
public sealed class NotificationService(
    INotificationRepository notifications,
    IRealtimePublisher realtime,
    IEnumerable<INotificationChannel> channels,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    ILogger<NotificationService> log) : INotificationService
{
    private const int MaxPageSize = 50;
    private static readonly JsonSerializerOptions PayloadJson = new(JsonSerializerDefaults.Web);

    public async Task NotifyAsync(INotifyRequest request, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var payloadJson = request.Payload is null ? null : JsonSerializer.Serialize(request.Payload, PayloadJson);
        var notification = Notification.Create(request.UserId, request.Type, request.Title, request.Body, payloadJson, now);

        await notifications.AddAsync(notification, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var inApp = NotificationDeliveryStatus.Sent;
        string? inAppError = null;
        try
        {
            await realtime.PublishToUserAsync(
                request.UserId,
                RealtimeEvents.NotificationCreated,
                new NotificationCreatedPayload(notification.Id, notification.Type, notification.Title, notification.Body, payloadJson, now),
                ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Kayıt zaten yazıldı; kullanıcı listeyi açınca görür.
            inApp = NotificationDeliveryStatus.Failed;
            inAppError = ex.Message;
            log.LogWarning(ex, "{Type} bildirimi anlık gönderilemedi ({NotificationId})", request.Type, notification.Id);
        }

        await notifications.AddDeliveryAsync(
            NotificationDelivery.Create(notification.Id, NotificationChannelType.InApp, inApp, 1, inAppError, now), ct);

        foreach (var channelType in request.Channels.Where(c => c != NotificationChannelType.InApp).Distinct())
        {
            var channel = channels.FirstOrDefault(c => c.Channel == channelType);
            if (channel is null)
            {
                await notifications.AddDeliveryAsync(
                    NotificationDelivery.Create(notification.Id, channelType, NotificationDeliveryStatus.Skipped, 1, "Kanal yapılandırılmadı.", now), ct);
                continue;
            }

            var result = await channel.SendAsync(new Message(notification, null), ct);
            await notifications.AddDeliveryAsync(
                NotificationDelivery.Create(notification.Id, channelType, result.Status, result.AttemptCount, result.Error, now), ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<Result<PagedResult<NotificationResponse>>> ListMineAsync(IPagedQuery query, CancellationToken ct)
    {
        var paging = new Paging(Math.Max(1, query.Page), Math.Clamp(query.PageSize, 1, MaxPageSize));
        return await notifications.ListByUserAsync(currentUser.UserId, paging, ct);
    }

    public Task<int> GetUnreadCountAsync(CancellationToken ct) =>
        notifications.CountUnreadAsync(currentUser.UserId, ct);

    public async Task<Result> MarkReadAsync(Guid notificationId, CancellationToken ct)
    {
        if (!await notifications.MarkReadAsync(currentUser.UserId, notificationId, clock.UtcNow, ct))
            return Errors.Notifications.NotFound; // başkasının bildirimi de bulunamadı sayılır

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> MarkAllReadAsync(CancellationToken ct)
    {
        if (await notifications.MarkAllReadAsync(currentUser.UserId, clock.UtcNow, ct) > 0)
            await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private sealed record Paging(int Page, int PageSize) : IPagedQuery;

    private sealed record Message(Notification Source, string? RecipientEmail) : INotificationMessage
    {
        public Guid NotificationId => Source.Id;

        public Guid UserId => Source.UserId;

        public string Type => Source.Type;

        public string Title => Source.Title;

        public string Body => Source.Body;

        public string? Link => null;

        public string? RecipientPhone => null;
    }
}
