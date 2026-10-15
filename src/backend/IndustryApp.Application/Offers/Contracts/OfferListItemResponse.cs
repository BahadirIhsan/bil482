using IndustryApp.Domain.Offers;

namespace IndustryApp.Application.Offers.Contracts;

/// <summary>Müşterinin rapor detayındaki teklif satırı (UC-04).</summary>
public sealed record OfferListItemResponse(
    Guid Id,
    Guid MechanicId,
    string WorkshopName,
    decimal TotalPrice,
    string Currency,
    short EstimatedDays,
    string? Note,
    OfferStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<OfferPriceLineResponse> PriceItems);

/// <summary>Teklifin fiyat kırılımındaki satır.</summary>
public sealed record OfferPriceLineResponse(PriceItemType ItemType, string? Description, decimal Amount);
