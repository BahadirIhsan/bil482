using IndustryApp.Application.Common;
using IndustryApp.Application.Offers.Contracts;

namespace IndustryApp.Application.Offers;

/// <summary>
/// Teklif işlemleri. v0.2'de yalnızca müşterinin teklif listesi (UC-04) vardır; teklif verme, güncelleme,
/// geri çekme ve ustanın listeleri UC-03 ile v0.3'te eklenir.
/// </summary>
public interface IOfferService
{
    Task<Result<IReadOnlyList<OfferListItemResponse>>> ListForReportAsync(IReportOffersQuery query, CancellationToken ct);
}
