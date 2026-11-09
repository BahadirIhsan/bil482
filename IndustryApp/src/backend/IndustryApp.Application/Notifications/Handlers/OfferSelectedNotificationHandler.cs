using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Notifications;

namespace IndustryApp.Application.Notifications.Handlers;

/// <summary>
/// UC-04 · Seçim sonrası (PAT-03.1): kazanan ustaya OFFER_SELECTED, diğer teklif verenlere OFFER_NOT_SELECTED.
/// Seçim servisi bu handler'ı bilmez; commit sonrası kuyruktan çalışır, HTTP yanıtını bekletmez.
/// </summary>
public sealed class OfferSelectedNotificationHandler(INotificationService notifications) : IEventHandler<OfferSelectedEvent>
{
    public async Task HandleAsync(OfferSelectedEvent e, CancellationToken ct)
    {
        var payload = new { reportId = e.ReportId, offerId = e.SelectedOfferId };

        await notifications.NotifyAsync(new NotifyRequest(
            e.WinnerMechanicId,
            NotificationType.OfferSelected,
            "Teklifiniz seçildi",
            "Müşteri teklifinizi seçti. İletişim bilgilerini eşleşme ekranında görebilirsiniz.",
            payload), ct);

        foreach (var mechanicId in e.OtherMechanicIds.Distinct().Where(id => id != e.WinnerMechanicId))
        {
            await notifications.NotifyAsync(new NotifyRequest(
                mechanicId,
                NotificationType.OfferNotSelected,
                "Başka bir teklif seçildi",
                "Müşteri bu rapor için başka bir ustanın teklifini seçti.",
                new { reportId = e.ReportId }), ct);
        }
    }
}
