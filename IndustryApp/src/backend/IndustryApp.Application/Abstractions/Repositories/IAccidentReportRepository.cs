using IndustryApp.Application.Common;
using IndustryApp.Application.Reports.Contracts;
using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface IAccidentReportRepository
{
    Task AddAsync(AccidentReport report, CancellationToken ct);

    /// <summary>Yalnızca rapor satırı.</summary>
    Task<AccidentReport?> GetAsync(Guid id, CancellationToken ct);

    /// <summary>Hasar kalemleri, fotoğraflar ve eşleşmelerle birlikte.</summary>
    Task<AccidentReport?> GetWithDetailsAsync(Guid id, CancellationToken ct);

    /// <summary>Teklifler (fiyat kalemleriyle) ve eşleşmelerle birlikte; seçim, iptal ve tamamlama için.</summary>
    Task<AccidentReport?> GetWithOffersAsync(Guid id, CancellationToken ct);

    Task<PagedResult<ReportListItemResponse>> ListByCustomerAsync(Guid customerId, IPagedQuery paging, CancellationToken ct);

    Task<int> CountOpenByCustomerAsync(Guid customerId, CancellationToken ct);

    /// <summary>Süresi dolmuş Open raporlar, teklifleriyle birlikte, en eskiden başlayarak.</summary>
    Task<IReadOnlyList<AccidentReport>> ListExpiredOpenAsync(DateTimeOffset now, int batchSize, CancellationToken ct);
}
