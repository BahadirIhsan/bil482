using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IndustryApp.Application.Reports.Expiration;

/// <summary>
/// Teklif seçilmeden süresi dolan raporlar: geçiş kararı State nesnesindedir (OpenState.Expire, PAT-04.1);
/// bekleyen teklifler kapanır, geçmişe sistem kaydı yazılır. ReportExpiredEvent commit sonrası müşteriye
/// REPORT_EXPIRED bildirimini başlatır. Aynı anda müşteri seçim yaparsa xmin çakışması o turu atlatır; bir
/// sonraki turda rapor artık Open değildir.
/// </summary>
public sealed class ReportExpirationService(
    IAccidentReportRepository reports,
    IReportStatusHistoryRepository history,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    IOptions<ReportsOptions> options,
    ILogger<ReportExpirationService> log) : IReportExpirationService
{
    public async Task<int> ExpireDueAsync(CancellationToken ct)
    {
        var now = clock.UtcNow;
        var due = await reports.ListExpiredOpenAsync(now, options.Value.ExpirationBatchSize, ct);
        if (due.Count == 0)
            return 0;

        foreach (var report in due)
        {
            report.Expire(now);
            foreach (var offer in report.Offers.Where(o => o.Status == OfferStatus.Pending))
                offer.Close(now);

            await history.AddAsync(
                ReportStatusHistory.Create(report.Id, ReportStatus.Open, report.Status, ReportCommandNames.ExpireReport, null, null, now),
                ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        log.LogInformation("{Count} raporun süresi doldu", due.Count);
        return due.Count;
    }
}
