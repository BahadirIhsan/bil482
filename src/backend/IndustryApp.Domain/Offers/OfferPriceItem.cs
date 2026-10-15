using IndustryApp.Domain.Common;
using IndustryApp.Domain.Offers.Pricing;

namespace IndustryApp.Domain.Offers;

/// <summary>Teklifin fiyat kırılımındaki bir satır; Decorator zincirinin Lines çıktısından üretilir.</summary>
public sealed class OfferPriceItem : BaseEntity<long>
{
    private OfferPriceItem()
    {
    }

    internal OfferPriceItem(Guid offerId, PriceLine line, short sortOrder)
    {
        OfferId = offerId;
        ItemType = line.Type;
        Description = string.IsNullOrWhiteSpace(line.Description) ? null : line.Description.Trim();
        Amount = line.Amount;
        SortOrder = sortOrder;
    }

    public Guid OfferId { get; private set; }

    public PriceItemType ItemType { get; private set; }

    public string? Description { get; private set; }

    public decimal Amount { get; private set; }

    public short SortOrder { get; private set; }
}
