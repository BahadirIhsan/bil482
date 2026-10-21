namespace IndustryApp.Domain.Offers.Pricing;

/// <summary>Decorator zincirinin bileşen arayüzü: toplam ve kalem kırılımı.</summary>
public interface IPriceComponent
{
    decimal Total { get; }

    IReadOnlyList<PriceLine> Lines { get; }
}

public sealed record PriceLine(PriceItemType Type, string? Description, decimal Amount);
