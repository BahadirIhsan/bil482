using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Reports.Contracts;

/// <summary>"Raporlarım" listesindeki satır: durum, teklif sayısı, en düşük teklif, oluşturulma tarihi.</summary>
public sealed record ReportListItemResponse(
    Guid Id,
    ReportStatus Status,
    string City,
    string? District,
    string VehicleTypeName,
    int OfferCount,
    decimal? LowestOfferPrice,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);
