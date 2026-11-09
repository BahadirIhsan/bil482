using FluentAssertions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Notifications;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IndustryApp.UnitTests.Application.Notifications;

/// <summary>T-9.3 bildirim hattı: kayıt → notification.created → kanallar → teslim kayıtları.</summary>
public sealed class NotificationServiceTests
{
    private readonly ServiceTestContext _ctx = new();
    private readonly INotificationRepository _repository = Substitute.For<INotificationRepository>();
    private readonly IRealtimePublisher _realtime = Substitute.For<IRealtimePublisher>();
    private readonly List<Notification> _saved = new();
    private readonly List<NotificationDelivery> _deliveries = new();
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _repository.When(r => r.AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>()))
            .Do(call => _saved.Add(call.Arg<Notification>()));
        _repository.When(r => r.AddDeliveryAsync(Arg.Any<NotificationDelivery>(), Arg.Any<CancellationToken>()))
            .Do(call => _deliveries.Add(call.Arg<NotificationDelivery>()));
        _service = new NotificationService(
            _repository, _realtime, [], _ctx.UnitOfWork, _ctx.CurrentUser, _ctx.Clock, NullLogger<NotificationService>.Instance);
    }

    [Fact]
    public async Task Notify_SavesThenPublishesAndRecordsInAppDelivery()
    {
        var userId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        await _service.NotifyAsync(
            new NotifyRequest(userId, NotificationType.OfferSelected, "Başlık", "Metin", new { reportId }), CancellationToken.None);

        var notification = _saved.Should().ContainSingle().Subject;
        notification.UserId.Should().Be(userId);
        notification.Payload.Should().Be($"{{\"reportId\":\"{reportId}\"}}");
        await _realtime.Received(1).PublishToUserAsync(
            userId, RealtimeEvents.NotificationCreated,
            Arg.Is<NotificationCreatedPayload>(p => p.Id == notification.Id && p.Type == NotificationType.OfferSelected),
            Arg.Any<CancellationToken>());
        _deliveries.Should().ContainSingle(d => d.Channel == NotificationChannelType.InApp && d.Status == NotificationDeliveryStatus.Sent);
        await _ctx.UnitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Notify_RealtimeFails_KeepsNotificationAndRecordsFailedDelivery()
    {
        _realtime.PublishToUserAsync(default, default!, default!, default).ThrowsAsyncForAnyArgs(new IOException("bağlantı yok"));

        await _service.NotifyAsync(new NotifyRequest(Guid.NewGuid(), NotificationType.JobCompleted, "B", "M"), CancellationToken.None);

        _saved.Should().ContainSingle();
        _deliveries.Should().ContainSingle(d => d.Channel == NotificationChannelType.InApp && d.Status == NotificationDeliveryStatus.Failed);
    }

    [Fact]
    public async Task Notify_EmailRequestedWithoutChannel_RecordsSkipped()
    {
        await _service.NotifyAsync(
            new NotifyRequest(Guid.NewGuid(), NotificationType.OfferSelected, "B", "M", null, [NotificationChannelType.Email]),
            CancellationToken.None);

        _deliveries.Should().Contain(d => d.Channel == NotificationChannelType.Email && d.Status == NotificationDeliveryStatus.Skipped);
    }

    [Fact]
    public async Task MarkRead_OtherUsersNotification_ReturnsNotFound()
    {
        _repository.MarkReadAsync(_ctx.UserId, Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _service.MarkReadAsync(Guid.NewGuid(), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.NotificationNotFound);
        await _ctx.UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListMine_ClampsPageSizeAndUsesCurrentUser()
    {
        _repository.ListByUserAsync(default, default!, default).ReturnsForAnyArgs(new PagedResult<NotificationResponse>([], 1, 50, 0));

        await _service.ListMineAsync(new Paging(0, 500), CancellationToken.None);

        await _repository.Received(1).ListByUserAsync(
            _ctx.UserId, Arg.Is<IPagedQuery>(p => p.Page == 1 && p.PageSize == 50), Arg.Any<CancellationToken>());
    }

    private sealed record Paging(int Page, int PageSize) : IPagedQuery;
}
