using IndustryApp.Application.Common;
using IndustryApp.Application.Offers.Contracts;
using IndustryApp.Application.Offers.Sorting;
using IndustryApp.Domain.Offers;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface IOfferRepository
{
    Task AddAsync(Offer offer, CancellationToken ct);

    /// <summary>Teklifi fiyat kalemleriyle yükler.</summary>
    Task<Offer?> GetAsync(Guid id, CancellationToken ct);

    Task<Offer?> GetByReportAndMechanicAsync(Guid reportId, Guid mechanicId, CancellationToken ct);

    /// <summary>Müşteri için: Pending, Accepted ve NotSelected teklifler, verilen stratejiyle sıralı.</summary>
    Task<IReadOnlyList<OfferListItemResponse>> ListForReportAsync(Guid reportId, IOfferSortStrategy sort, CancellationToken ct);

    /// <summary>Usta için: kendi teklifleri; status null ise tüm durumlar.</summary>
    Task<PagedResult<MyOfferListItemResponse>> ListByMechanicAsync(
        Guid mechanicId, OfferStatus? status, IPagedQuery paging, CancellationToken ct);

    /// <summary>Raporun tüm teklifleri (takip edilen entity olarak).</summary>
    Task<IReadOnlyList<Offer>> ListByReportAsync(Guid reportId, CancellationToken ct);
}
