using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Reports.Contracts;

/// <summary>
/// Ustanın iş listesindeki satır. Kapalı teklif kuralı (K-06): başka ustaların teklifleri ve müşterinin
/// iletişim bilgisi bu modelde hiç yoktur; yalnızca ustanın kendi teklifi vardır.
/// </summary>
public sealed record MechanicJobListItemResponse(
    Guid ReportId,
    ReportStatus ReportStatus,
    string City,
    string? District,
    string VehicleTypeName,
    int DamageCount,
    DateTimeOffset NotifiedAt,
    DateTimeOffset? ViewedAt,
    DateTimeOffset ExpiresAt,
    OfferStatus? MyOfferStatus,
    decimal? MyOfferTotalPrice);
