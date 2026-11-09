using IndustryApp.Domain.Offers;

namespace IndustryApp.Application.Offers.Sorting;

/// <summary>En düşük toplam fiyat önce; eşitlikte önce verilen teklif (PAT-04.4).</summary>
public sealed class PriceAscendingSort : IOfferSortStrategy
{
    public string Key => OfferSortKeys.Price;

    public IQueryable<Offer> Apply(IQueryable<Offer> offers) =>
        offers.OrderBy(o => o.TotalPrice).ThenBy(o => o.CreatedAt);
}
