using IndustryApp.Application.Selection;
using IndustryApp.Application.Selection.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>UC-04 teklif seçimi (endpoint 29, 30, 31). Sahiplik ve durum kontrolleri serviste ve State'tedir.</summary>
[Route("api/reports/{reportId:guid}/selection")]
[Authorize]
public sealed class OfferSelectionController(IOfferSelectionService selection) : ApiControllerBase
{
    /// <summary>29 · Müşteri bir teklifi seçer.</summary>
    [HttpPost]
    [Authorize(Policy = "CustomerOnly")]
    [ProducesResponseType<SelectionResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Select(Guid reportId, SelectOfferBody body, CancellationToken ct) =>
        ToActionResult(await selection.SelectAsync(new SelectOfferRequest(reportId, body.OfferId), ct));

    /// <summary>30 · Müşteri seçimi süre içinde geri alır.</summary>
    [HttpDelete]
    [Authorize(Policy = "CustomerOnly")]
    [ProducesResponseType<SelectionStatusResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Undo(Guid reportId, CancellationToken ct) =>
        ToActionResult(await selection.UndoAsync(new UndoSelectionRequest(reportId), ct));

    /// <summary>31 · Raporun seçim durumu (müşteri veya raporla eşleşmiş usta).</summary>
    [HttpGet]
    [ProducesResponseType<SelectionStatusResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid reportId, CancellationToken ct) =>
        ToActionResult(await selection.GetAsync(reportId, ct));
}

/// <summary>POST /selection gövdesi; rapor kimliği yoldan gelir.</summary>
public sealed record SelectOfferBody(Guid OfferId);
