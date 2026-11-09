using IndustryApp.Application.Common;
using IndustryApp.Application.Reports.Contracts;

namespace IndustryApp.Application.Reports;

/// <summary>
/// Müşterinin rapor işlemleri. Bu fazda UC-04 için detay, tamamlama ve iptal vardır (FR-04.5);
/// rapor oluşturma ve listeleme (UC-02) Faz 3'te bu arayüze eklenir.
/// </summary>
public interface IReportService
{
    Task<Result<ReportDetailResponse>> GetForCustomerAsync(Guid reportId, CancellationToken ct);

    Task<Result> CompleteAsync(Guid reportId, CancellationToken ct);

    Task<Result> CancelAsync(Guid reportId, CancellationToken ct);
}
