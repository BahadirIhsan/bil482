using FluentAssertions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Notifications;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Application.Notifications.Handlers;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Common.Options;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Notifications;
using IndustryApp.Domain.Reports;
using IndustryApp.UnitTests.Domain;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IndustryApp.UnitTests.Application.Notifications;

/// <summary>
/// UC-04 olay handler'ları (PAT-03.1): seçim servisi kimin bildirileceğini bilmez; handler'lar kazanana
/// OFFER_SELECTED, diğerlerine OFFER_NOT_SELECTED, durum değişiminde herkese report.statusChanged gönderir.
/// </summary>
public sealed class Uc04NotificationHandlerTests
{
    private readonly INotificationService _notifications = Substitute.For<INotificationService>();
    private readonly List<INotifyRequest> _sent = new();

    public Uc04NotificationHandlerTests() =>
        _notifications.When(n => n.NotifyAsync(Arg.Any<INotifyRequest>(), Arg.Any<CancellationToken>()))
            .Do(call => _sent.Add(call.Arg<INotifyRequest>()));

    [Fact]
    public async Task OfferSelected_NotifiesWinnerAndOthers()
    {
        var winner = Guid.NewGuid();
        var others = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var e = new OfferSelectedEvent(Guid.NewGuid(), Guid.NewGuid(), winner, others, Guid.NewGuid(), DomainTestData.Now);

        await new OfferSelectedNotificationHandler(_notifications).HandleAsync(e, CancellationToken.None);

        _sent.Should().ContainSingle(r => r.UserId == winner && r.Type == NotificationType.OfferSelected);
        _sent.Where(r => r.Type == NotificationType.OfferNotSelected).Select(r => r.UserId).Should().BeEquivalentTo(others);
        _sent.Should().NotContain(r => r.UserId == e.CustomerId);
    }

    [Fact]
    public async Task SelectionUndone_NotifiesEveryBiddingMechanic()
    {
        var mechanics = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        await new OfferSelectionUndoneNotificationHandler(_notifications)
            .HandleAsync(new OfferSelectionUndoneEvent(Guid.NewGuid(), mechanics, DomainTestData.Now), CancellationToken.None);

        _sent.Should().HaveCount(3).And.OnlyContain(r => r.Type == NotificationType.SelectionUndone);
    }

    [Fact]
    public async Task ReportCompletedAndCancelled_NotifyMechanics()
    {
        var winner = Guid.NewGuid();
        var bidders = new[] { Guid.NewGuid(), Guid.NewGuid() };

        await new ReportCompletedNotificationHandler(_notifications)
            .HandleAsync(new ReportCompletedEvent(Guid.NewGuid(), winner, DomainTestData.Now), CancellationToken.None);
        await new ReportCancelledNotificationHandler(_notifications)
            .HandleAsync(new ReportCancelledEvent(Guid.NewGuid(), bidders, DomainTestData.Now), CancellationToken.None);

        _sent.Should().ContainSingle(r => r.UserId == winner && r.Type == NotificationType.JobCompleted);
        _sent.Where(r => r.Type == NotificationType.ReportCancelled).Select(r => r.UserId).Should().BeEquivalentTo(bidders);
    }

    [Fact]
    public async Task ReportStatusRealtime_PublishesCommittedStatusToCustomerAndBidders()
    {
        var ctx = new ServiceTestContext();
        var report = DomainTestData.OpenReport();
        var chosen = DomainTestData.OfferFor(report, Guid.NewGuid());
        var other = DomainTestData.OfferFor(report, Guid.NewGuid(), labor: 4200m);
        new ReportCommandFactory(Options.Create(new OffersOptions { SelectionUndoWindowMinutes = 10 }))
            .CreateSelectOffer(report, chosen, report.CustomerId, DomainTestData.Now.AddHours(1)).Execute();
        ctx.Returns(report);
        var realtime = Substitute.For<IRealtimePublisher>();

        await new ReportStatusRealtimeHandler(ctx.Reports, realtime).HandleAsync(
            new OfferSelectedEvent(report.Id, chosen.Id, chosen.MechanicId, [other.MechanicId], report.CustomerId, DomainTestData.Now),
            CancellationToken.None);

        foreach (var userId in new[] { report.CustomerId, chosen.MechanicId, other.MechanicId })
        {
            await realtime.Received(1).PublishToUserAsync(
                userId, RealtimeEvents.ReportStatusChanged,
                Arg.Is<ReportStatusChangedPayload>(p => p.Status == nameof(ReportStatus.OfferSelected) && p.SelectedOfferId == chosen.Id),
                Arg.Any<CancellationToken>());
        }
    }
}
