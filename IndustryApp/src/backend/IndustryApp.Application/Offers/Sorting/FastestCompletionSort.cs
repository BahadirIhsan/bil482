using IndustryApp.Domain.Offers;

namespace IndustryApp.Application.Offers.Sorting;

/// <summary>En kısa tahmini süre önce; eşitlikte daha ucuz teklif (PAT-04.4).</summary>
public sealed class FastestCompletionSort : IOfferSortStrategy
{
    public string Key => OfferSortKeys.Fastest;

    public IQueryable<Offer> Apply(IQueryable<Offer> offers) =>
        offers.OrderBy(o => o.EstimatedDays).ThenBy(o => o.TotalPrice);
}
