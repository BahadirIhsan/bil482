using IndustryApp.Application.Common;
using IndustryApp.Application.Reports.Contracts;
using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface IReportMatchRepository
{
    Task AddRangeAsync(IEnumerable<ReportMatch> matches, CancellationToken ct);

    Task<bool> IsMatchedAsync(Guid reportId, Guid mechanicId, CancellationToken ct);

    Task<PagedResult<MechanicJobListItemResponse>> ListJobsForMechanicAsync(
        Guid mechanicId, MechanicJobFilter filter, IPagedQuery paging, CancellationToken ct);

    /// <summary>Ustanın raporu ilk açtığı anı işaretler; eşleşme yoksa false.</summary>
    Task<bool> MarkViewedAsync(Guid reportId, Guid mechanicId, DateTimeOffset now, CancellationToken ct);
}
