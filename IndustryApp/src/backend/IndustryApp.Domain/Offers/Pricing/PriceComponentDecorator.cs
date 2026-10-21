using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Offers.Pricing;

// [UC-03][PAT-03.4] Decorator (fiyat)
// Niyet   : Teklif fiyatını işçilik + isteğe bağlı parça, boya, garanti ve diğer kalemlerle katman katman kurmak.
// Neden   : Müşteri tek bir sayı değil kırılım görmelidir; kalemleri tek bir sınıfta if'lerle toplamak
//           her yeni kalem türünde aynı kodu değiştirirdi.
// Kazanım : Toplam her zaman kalemlerin toplamına eşittir; yeni kalem türü tek bir dekoratör sınıfıdır.
// Metrik  : Total = satırların toplamı (PriceDecorators_TotalEqualsSumOfLines, TC-03.3.1).
public abstract class PriceComponentDecorator : IPriceComponent
{
    private readonly IPriceComponent _inner;
    private readonly PriceLine _line;

    protected PriceComponentDecorator(IPriceComponent inner, PriceItemType type, decimal amount, string? description)
    {
        if (amount < 0)
            throw new DomainException(ErrorCodes.PriceAmountInvalid, "Fiyat kalemi tutarı negatif olamaz.");

        _inner = inner;
        _line = new PriceLine(type, description, amount);
    }

    public decimal Total => _inner.Total + _line.Amount;

    public IReadOnlyList<PriceLine> Lines => [.. _inner.Lines, _line];
}

public sealed class PartsCost(IPriceComponent inner, decimal amount, string? description)
    : PriceComponentDecorator(inner, PriceItemType.Parts, amount, description);

public sealed class PaintCost(IPriceComponent inner, decimal amount, string? description)
    : PriceComponentDecorator(inner, PriceItemType.Paint, amount, description);

public sealed class WarrantyCost(IPriceComponent inner, decimal amount, string? description)
    : PriceComponentDecorator(inner, PriceItemType.Warranty, amount, description);

public sealed class OtherCost(IPriceComponent inner, decimal amount, string? description)
    : PriceComponentDecorator(inner, PriceItemType.Other, amount, description);
