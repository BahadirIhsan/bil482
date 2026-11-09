using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Notifications;

namespace IndustryApp.Application.Notifications.Handlers;

/// <summary>UC-04 · Seçim geri alındı (FR-04.3): teklif veren tüm ustalara SELECTION_UNDONE.</summary>
public sealed class OfferSelectionUndoneNotificationHandler(INotificationService notifications) : IEventHandler<OfferSelectionUndoneEvent>
{
    public async Task HandleAsync(OfferSelectionUndoneEvent e, CancellationToken ct)
    {
        foreach (var mechanicId in e.MechanicIds.Distinct())
        {
            await notifications.NotifyAsync(new NotifyRequest(
                mechanicId,
                NotificationType.SelectionUndone,
                "Müşteri seçimini geri aldı",
                "Rapor yeniden değerlendirmeye açıldı; teklifiniz tekrar beklemede.",
                new { reportId = e.ReportId }), ct);
        }
    }
}
