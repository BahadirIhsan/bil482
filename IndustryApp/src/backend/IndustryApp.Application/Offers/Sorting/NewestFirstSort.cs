using IndustryApp.Domain.Offers;

namespace IndustryApp.Application.Offers.Sorting;

/// <summary>En son verilen veya güncellenen teklif önce (PAT-04.4).</summary>
public sealed class NewestFirstSort : IOfferSortStrategy
{
    public string Key => OfferSortKeys.Newest;

    public IQueryable<Offer> Apply(IQueryable<Offer> offers) =>
        offers.OrderByDescending(o => o.UpdatedAt ?? o.CreatedAt).ThenBy(o => o.TotalPrice);
}
