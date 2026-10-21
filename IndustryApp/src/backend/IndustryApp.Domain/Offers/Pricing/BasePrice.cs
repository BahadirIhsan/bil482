using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Offers.Pricing;

/// <summary>Zincirin çekirdeği: işçilik (Labor) kalemi. Her teklifte zorunludur.</summary>
public sealed class BasePrice : IPriceComponent
{
    public BasePrice(decimal laborAmount, string? description)
    {
        if (laborAmount < 0)
            throw new DomainException(ErrorCodes.PriceAmountInvalid, "İşçilik tutarı negatif olamaz.");

        Total = laborAmount;
        Lines = [new PriceLine(PriceItemType.Labor, description, laborAmount)];
    }

    public decimal Total { get; }

    public IReadOnlyList<PriceLine> Lines { get; }
}
