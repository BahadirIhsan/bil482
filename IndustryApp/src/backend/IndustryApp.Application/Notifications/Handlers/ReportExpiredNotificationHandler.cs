using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Notifications.Contracts;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Notifications;

namespace IndustryApp.Application.Notifications.Handlers;

/// <summary>UC-02 · Rapor teklif seçilmeden süresi doldu (FR-02.4): müşteriye REPORT_EXPIRED.</summary>
public sealed class ReportExpiredNotificationHandler(INotificationService notifications) : IEventHandler<ReportExpiredEvent>
{
    public Task HandleAsync(ReportExpiredEvent e, CancellationToken ct) =>
        notifications.NotifyAsync(new NotifyRequest(
            e.CustomerId,
            NotificationType.ReportExpired,
            "Raporunuzun süresi doldu",
            "Raporunuz için süre içinde teklif seçilmedi ve rapor kapandı. Dilerseniz yeni bir rapor oluşturabilirsiniz.",
            new { reportId = e.ReportId }), ct);
}
