using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Reports.Contracts;

/// <summary>
/// Müşterinin rapor detayı (endpoint 18). Teklif listesi ayrı endpoint'tedir (27); burada yalnızca seçim özeti
/// (SelectedOfferId, OfferSelectedAt, UndoDeadline) vardır.
/// </summary>
public sealed record ReportDetailResponse(
    Guid Id,
    ReportStatus Status,
    string City,
    string? District,
    string? AddressText,
    string? Description,
    string VehicleTypeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    Guid? SelectedOfferId,
    DateTimeOffset? OfferSelectedAt,
    DateTimeOffset? UndoDeadline,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt,
    IReadOnlyList<ReportDamageItemResponse> DamageItems,
    IReadOnlyList<Guid> PhotoIds);

/// <summary>Rapordaki hasar kalemi; kategori adı katalogdan okunur.</summary>
public sealed record ReportDamageItemResponse(int DamageCategoryId, string CategoryName, DamageSeverity Severity, string? Note);
