using IndustryApp.Application.Catalog.Contracts;

namespace IndustryApp.Application.Catalog;

/// <summary>
/// Anonim katalog okumaları: hasar ağacı (11), araç türleri (12) ve sözleşmeler (13). Ağaç ve araç türleri
/// Catalog:CacheMinutes süresince önbellektedir; admin değişikliği CatalogCacheKeys anahtarını temizler.
/// </summary>
public interface ICatalogService
{
    Task<IReadOnlyList<DamageCategoryNodeResponse>> GetDamageTreeAsync(CancellationToken ct);

    Task<IReadOnlyList<VehicleTypeResponse>> GetVehicleTypesAsync(CancellationToken ct);

    /// <summary>Rolün güncel sözleşmeleri; rol tanımsızsa boş liste.</summary>
    Task<IReadOnlyList<LegalDocumentResponse>> GetLegalDocumentsAsync(string? role, CancellationToken ct);
}
