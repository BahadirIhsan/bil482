using FluentAssertions;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Offers.Pricing;

namespace IndustryApp.UnitTests.Domain;

/// <summary>PAT-03.4 Decorator (fiyat): toplam her zaman kalemlerin toplamıdır.</summary>
public sealed class PriceDecoratorTests
{
    // TC-03.3.1
    [Fact]
    public void PriceDecorators_TotalEqualsSumOfLines()
    {
        IPriceComponent price = new BasePrice(2000m, "İşçilik");
        price = new PartsCost(price, 1500.50m, "Ön tampon");
        price = new PaintCost(price, 800m, null);
        price = new WarrantyCost(price, 250m, "6 ay garanti");
        price = new OtherCost(price, 99.50m, "Çekici");

        price.Total.Should().Be(4650m);
        price.Total.Should().Be(price.Lines.Sum(l => l.Amount));
        price.Lines.Select(l => l.Type).Should().Equal(
            PriceItemType.Labor, PriceItemType.Parts, PriceItemType.Paint, PriceItemType.Warranty, PriceItemType.Other);
    }
}
