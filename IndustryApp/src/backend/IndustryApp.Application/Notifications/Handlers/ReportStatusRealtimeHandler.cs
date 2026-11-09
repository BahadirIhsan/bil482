using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Domain.Events;

namespace IndustryApp.Application.Notifications.Handlers;

/// <summary>report.statusChanged yükü (T-12). Status metin olarak gider (Open, OfferSelected …).</summary>
public sealed record ReportStatusChangedPayload(Guid ReportId, string Status, Guid? SelectedOfferId);

/// <summary>
/// UC-04 · Seçim, geri alma, tamamlama ve iptalde rapor sahibine ve teklif vermiş ustalara report.statusChanged
/// gönderir; açık ekranlar sayfa yenilenmeden güncellenir (PR-04.1). Durum, commit edilmiş rapordan okunur;
/// böylece olaylar sırasız işlense bile istemci en güncel durumu görür. Asıl kaynak her zaman REST'tir.
/// </summary>
public sealed class ReportStatusRealtimeHandler(IAccidentReportRepository reports, IRealtimePublisher realtime)
    : IEventHandler<OfferSelectedEvent>,
      IEventHandler<OfferSelectionUndoneEvent>,
      IEventHandler<ReportCompletedEvent>,
      IEventHandler<ReportCancelledEvent>
{
    public Task HandleAsync(OfferSelectedEvent e, CancellationToken ct) => PublishAsync(e.ReportId, ct);

    public Task HandleAsync(OfferSelectionUndoneEvent e, CancellationToken ct) => PublishAsync(e.ReportId, ct);

    public Task HandleAsync(ReportCompletedEvent e, CancellationToken ct) => PublishAsync(e.ReportId, ct);

    public Task HandleAsync(ReportCancelledEvent e, CancellationToken ct) => PublishAsync(e.ReportId, ct);

    private async Task PublishAsync(Guid reportId, CancellationToken ct)
    {
        var report = await reports.GetWithOffersAsync(reportId, ct);
        if (report is null)
            return;

        var payload = new ReportStatusChangedPayload(report.Id, report.Status.ToString(), report.SelectedOfferId);
        var recipients = report.Offers.Select(o => o.MechanicId).Append(report.CustomerId).Distinct();

        foreach (var userId in recipients)
            await realtime.PublishToUserAsync(userId, RealtimeEvents.ReportStatusChanged, payload, ct);
    }
}
