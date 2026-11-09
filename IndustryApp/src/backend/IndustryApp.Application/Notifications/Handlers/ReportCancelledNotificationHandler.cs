using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Notifications;

namespace IndustryApp.Application.Notifications.Handlers;

/// <summary>UC-04 · Rapor iptal edildi (FR-04.5): teklif vermiş ustalara REPORT_CANCELLED.</summary>
public sealed class ReportCancelledNotificationHandler(INotificationService notifications) : IEventHandler<ReportCancelledEvent>
{
    public async Task HandleAsync(ReportCancelledEvent e, CancellationToken ct)
    {
        foreach (var mechanicId in e.MechanicIdsWithOffers.Distinct())
        {
            await notifications.NotifyAsync(new NotifyRequest(
                mechanicId,
                NotificationType.ReportCancelled,
                "Rapor iptal edildi",
                "Müşteri teklif verdiğiniz raporu iptal etti.",
                new { reportId = e.ReportId }), ct);
        }
    }
}
