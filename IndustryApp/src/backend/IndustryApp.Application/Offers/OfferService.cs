using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Offers.Contracts;
using IndustryApp.Application.Offers.Sorting;

namespace IndustryApp.Application.Offers;

/// <summary>
/// Strateji deseninin istemcisi (PAT-04.4): isteğin anahtarına uyan IOfferSortStrategy'yi seçer, bulamazsa fiyata
/// düşer. Sıralama repository sorgusunda SQL ORDER BY olarak çalışır; servis hiçbir zaman bellekte sıralamaz.
/// Kapalı teklif kuralı (K-06): liste yalnızca rapor sahibine açıktır.
/// </summary>
public sealed class OfferService(
    IAccidentReportRepository reports,
    IOfferRepository offers,
    IEnumerable<IOfferSortStrategy> strategies,
    ICurrentUser currentUser) : IOfferService
{
    private readonly IReadOnlyDictionary<string, IOfferSortStrategy> _strategies =
        strategies.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

    public async Task<Result<IReadOnlyList<OfferListItemResponse>>> ListForReportAsync(IReportOffersQuery query, CancellationToken ct)
    {
        var report = await reports.GetAsync(query.ReportId, ct);
        if (report is null)
            return Errors.Reports.NotFound;
        if (!report.IsOwnedBy(currentUser.UserId))
            return Errors.Reports.NotOwner;

        var list = await offers.ListForReportAsync(report.Id, Resolve(query.SortKey), ct);
        return Result.Success(list);
    }

    /// <summary>Anahtara uyan strateji; boş veya bilinmeyen anahtar için varsayılan (price).</summary>
    public IOfferSortStrategy Resolve(string? sortKey) =>
        sortKey is not null && _strategies.TryGetValue(sortKey.Trim(), out var strategy)
            ? strategy
            : _strategies[OfferSortKeys.Default];
}
