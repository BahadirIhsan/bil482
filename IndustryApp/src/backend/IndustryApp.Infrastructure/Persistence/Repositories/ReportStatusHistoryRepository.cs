using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class ReportStatusHistoryRepository(AppDbContext db) : IReportStatusHistoryRepository
{
    public async Task AddAsync(ReportStatusHistory entry, CancellationToken ct) =>
        await db.ReportStatusHistory.AddAsync(entry, ct);

    public Task<ReportStatusHistory?> GetLatestAsync(Guid reportId, string commandName, CancellationToken ct) =>
        db.ReportStatusHistory.AsNoTracking()
            .Where(h => h.ReportId == reportId && h.CommandName == commandName)
            .OrderByDescending(h => h.OccurredAt).ThenByDescending(h => h.Id)
            .FirstOrDefaultAsync(ct);
}
