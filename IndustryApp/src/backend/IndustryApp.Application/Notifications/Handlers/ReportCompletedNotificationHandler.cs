using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Notifications;

namespace IndustryApp.Application.Notifications.Handlers;

/// <summary>UC-04 · İş tamamlandı (FR-04.5): kazanan ustaya JOB_COMPLETED.</summary>
public sealed class ReportCompletedNotificationHandler(INotificationService notifications) : IEventHandler<ReportCompletedEvent>
{
    public Task HandleAsync(ReportCompletedEvent e, CancellationToken ct) =>
        notifications.NotifyAsync(new NotifyRequest(
            e.WinnerMechanicId,
            NotificationType.JobCompleted,
            "İş tamamlandı",
            "Müşteri işi tamamlandı olarak işaretledi.",
            new { reportId = e.ReportId }), ct);
}
