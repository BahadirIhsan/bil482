using IndustryApp.Application.Offers;
using IndustryApp.Application.Offers.Contracts;
using IndustryApp.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>
/// UC-04 teklif listesi (endpoint 27). Sıralama anahtarı olduğu gibi servise geçer; stratejiyi OfferService seçer
/// (PAT-04.4), controller'da switch yoktur. Teklif verme/güncelleme/geri çekme UC-03 ile v0.3'te eklenir.
/// </summary>
[Route("api/reports/{reportId:guid}/offers")]
[Authorize(Policy = AuthPolicies.CustomerOnly)]
public sealed class OffersController(IOfferService offers) : ApiControllerBase
{
    /// <summary>27 · Raporun teklifleri; sort = price | fastest | newest (tanımsızsa price).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OfferListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(Guid reportId, [FromQuery] string? sort, CancellationToken ct) =>
        ToActionResult(await offers.ListForReportAsync(new ReportOffersQuery(reportId, sort), ct));
}
