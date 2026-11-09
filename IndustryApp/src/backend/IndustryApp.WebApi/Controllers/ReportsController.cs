using IndustryApp.Application.Reports;
using IndustryApp.Application.Reports.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>
/// Müşterinin rapor uç noktaları. Bu fazda UC-04 için 18, 19 ve 20 vardır; rapor oluşturma ve listeleme
/// (16, 17) UC-02 ile Faz 3'te eklenir.
/// </summary>
[Route("api/reports")]
[Authorize(Policy = "CustomerOnly")]
public sealed class ReportsController(IReportService reports) : ApiControllerBase
{
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
}
