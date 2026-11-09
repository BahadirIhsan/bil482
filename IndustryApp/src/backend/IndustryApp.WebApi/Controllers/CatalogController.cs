using IndustryApp.Application.Catalog;
using IndustryApp.Application.Catalog.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>Anonim katalog okumaları; kayıt formları için (endpoint 11, 13). Araç türleri (12) UC-02 ile gelir.</summary>
[Route("api/catalog")]
[AllowAnonymous]
public sealed class CatalogController(ICatalogService catalog) : ApiControllerBase
{
    /// <summary>11 · Aktif hasar ağacı (3 seviye). Usta kaydında 1. seviye uzmanlık olarak seçilir.</summary>
    [HttpGet("damage-categories")]
    [ProducesResponseType<IReadOnlyList<DamageCategoryNodeResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> DamageCategories(CancellationToken ct) =>
        Ok(await catalog.GetDamageTreeAsync(ct));

    /// <summary>13 · Rolün güncel sözleşmeleri (Customer veya Mechanic).</summary>
    [HttpGet("legal-documents")]
    [ProducesResponseType<IReadOnlyList<LegalDocumentResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> LegalDocuments([FromQuery] string? role, CancellationToken ct) =>
        Ok(await catalog.GetLegalDocumentsAsync(role, ct));
}
