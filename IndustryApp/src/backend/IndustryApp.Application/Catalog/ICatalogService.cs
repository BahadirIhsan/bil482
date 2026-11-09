using IndustryApp.Application.Catalog.Contracts;

namespace IndustryApp.Application.Catalog;

/// <summary>
/// Anonim katalog okumaları. v0.2'de kayıt formlarının ihtiyacı olan hasar ağacı (11) ve sözleşmeler (13) vardır;
/// araç türleri (12) rapor sihirbazıyla (UC-02) eklenir.
/// </summary>
public interface ICatalogService
{
    Task<IReadOnlyList<DamageCategoryNodeResponse>> GetDamageTreeAsync(CancellationToken ct);

    /// <summary>Rolün güncel sözleşmeleri; rol tanımsızsa boş liste.</summary>
    Task<IReadOnlyList<LegalDocumentResponse>> GetLegalDocumentsAsync(string? role, CancellationToken ct);
}
