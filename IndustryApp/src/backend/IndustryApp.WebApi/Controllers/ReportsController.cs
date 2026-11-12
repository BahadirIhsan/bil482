using IndustryApp.Application.Common;
using IndustryApp.Application.Reports;
using IndustryApp.Application.Reports.Contracts;
using IndustryApp.WebApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>Müşterinin rapor uç noktaları: oluşturma ve liste (16, 17 · UC-02), detay, iptal, tamamlama (18–20 · UC-04).</summary>
[Route("api/reports")]
[Authorize(Policy = "CustomerOnly")]
public sealed class ReportsController(IReportService reports) : ApiControllerBase
{
    /// <summary>16 · Raporu oluşturur; eşleştirme yanıttan sonra arka planda başlar.</summary>
    [HttpPost]
    [ProducesResponseType<CreateReportResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateReportRequest request, CancellationToken ct) =>
        ToActionResult(await reports.CreateAsync(request, ct),
            created => CreatedAtAction(nameof(Get), new { reportId = created.Id }, created));

    /// <summary>17 · Müşterinin raporları, en yeni önce (pageSize en fazla 100).</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ReportListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        ToActionResult(await reports.ListMineAsync(new PagedQuery(page, pageSize), ct));

    /// <summary>18 · Rapor detayı ve seçim özeti.</summary>
    [HttpGet("{reportId:guid}")]
    [ProducesResponseType<ReportDetailResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid reportId, CancellationToken ct) =>
        ToActionResult(await reports.GetForCustomerAsync(reportId, ct));

    /// <summary>19 · Raporu iptal eder; bekleyen teklifler kapanır.</summary>
    [HttpPost("{reportId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid reportId, CancellationToken ct) =>
        ToActionResult(await reports.CancelAsync(reportId, ct));

    /// <summary>20 · İşi tamamlandı olarak işaretler.</summary>
    [HttpPost("{reportId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Complete(Guid reportId, CancellationToken ct) =>
        ToActionResult(await reports.CompleteAsync(reportId, ct));

    private sealed record PagedQuery(int Page, int PageSize) : IPagedQuery;
}
