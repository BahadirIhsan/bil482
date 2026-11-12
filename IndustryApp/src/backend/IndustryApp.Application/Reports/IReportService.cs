using IndustryApp.Application.Common;
using IndustryApp.Application.Reports.Contracts;

namespace IndustryApp.Application.Reports;

/// <summary>
/// Müşterinin rapor işlemleri: oluşturma ve listeleme (UC-02, endpoint 16–17), detay, tamamlama ve iptal
/// (UC-04, endpoint 18–20).
/// </summary>
public interface IReportService
{
    Task<Result<CreateReportResponse>> CreateAsync(ICreateReportRequest request, CancellationToken ct);

    Task<Result<PagedResult<ReportListItemResponse>>> ListMineAsync(IPagedQuery query, CancellationToken ct);

    Task<Result<ReportDetailResponse>> GetForCustomerAsync(Guid reportId, CancellationToken ct);

    Task<Result> CompleteAsync(Guid reportId, CancellationToken ct);

    Task<Result> CancelAsync(Guid reportId, CancellationToken ct);
}
