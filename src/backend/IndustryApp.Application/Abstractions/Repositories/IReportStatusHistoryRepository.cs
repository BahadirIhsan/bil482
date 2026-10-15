using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface IReportStatusHistoryRepository
{
    Task AddAsync(ReportStatusHistory entry, CancellationToken ct);

    /// <summary>Raporun verilen komut adına sahip en son geçmiş kaydı (geri alma için).</summary>
    Task<ReportStatusHistory?> GetLatestAsync(Guid reportId, string commandName, CancellationToken ct);
}
