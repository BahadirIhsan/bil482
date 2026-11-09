using FluentAssertions;
using IndustryApp.Application.Offers.Sorting;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Offers.Pricing;
using IndustryApp.UnitTests.Domain;

namespace IndustryApp.UnitTests.Application.Offers;

/// <summary>PAT-04.4 · Her sıralama stratejisi için bir test (TC-04.1.2–4). Sorgu LINQ to Objects ile çalışır.</summary>
public sealed class OfferSortStrategyTests
{
    private readonly Offer _cheapSlowOld;
    private readonly Offer _midFastNew;
    private readonly Offer _expensiveFastMid;
    private readonly IQueryable<Offer> _offers;

    public OfferSortStrategyTests()
    {
        var report = DomainTestData.OpenReport();
        _cheapSlowOld = Create(report, 4000m, days: 5, minutesAfter: 0);
        _midFastNew = Create(report, 4500m, days: 2, minutesAfter: 30);
        _expensiveFastMid = Create(report, 5000m, days: 2, minutesAfter: 10);
        _offers = new[] { _expensiveFastMid, _cheapSlowOld, _midFastNew }.AsQueryable();
    }

    // TC-04.1.2
    [Fact]
    public void PriceAscendingSort_OrdersByTotalPrice()
    {
        var sorted = new PriceAscendingSort().Apply(_offers).ToList();

        sorted.Should().ContainInOrder(_cheapSlowOld, _midFastNew, _expensiveFastMid);
    }

    // TC-04.1.3
    [Fact]
    public void FastestCompletionSort_OrdersByDaysThenPrice()
    {
        var sorted = new FastestCompletionSort().Apply(_offers).ToList();

        sorted.Should().ContainInOrder(_midFastNew, _expensiveFastMid, _cheapSlowOld);
    }

    // TC-04.1.4
    [Fact]
    public void NewestFirstSort_OrdersByCreatedAtDescending()
    {
        var sorted = new NewestFirstSort().Apply(_offers).ToList();

        sorted.Should().ContainInOrder(_midFastNew, _expensiveFastMid, _cheapSlowOld);
    }

    [Fact]
    public void Strategies_HaveDistinctKeysMatchingApiValues()
    {
        IOfferSortStrategy[] strategies = [new PriceAscendingSort(), new FastestCompletionSort(), new NewestFirstSort()];

        strategies.Select(s => s.Key).Should().BeEquivalentTo([OfferSortKeys.Price, OfferSortKeys.Fastest, OfferSortKeys.Newest]);
    }

    private static Offer Create(IndustryApp.Domain.Reports.AccidentReport report, decimal labor, short days, int minutesAfter) =>
        Offer.Create(report, Guid.NewGuid(), new BasePrice(labor, "İşçilik"), days, null, DomainTestData.Now.AddMinutes(minutesAfter));
}
