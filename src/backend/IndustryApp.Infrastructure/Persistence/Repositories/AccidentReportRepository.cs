using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Reports.Contracts;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class AccidentReportRepository(AppDbContext db) : IAccidentReportRepository
{
    public async Task AddAsync(AccidentReport report, CancellationToken ct) =>
        await db.AccidentReports.AddAsync(report, ct);

    public Task<AccidentReport?> GetAsync(Guid id, CancellationToken ct) =>
        db.AccidentReports.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<AccidentReport?> GetWithDetailsAsync(Guid id, CancellationToken ct) =>
        db.AccidentReports
            .Include(r => r.DamageItems)
            .Include(r => r.Photos)
            .Include(r => r.Matches)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<AccidentReport?> GetWithOffersAsync(Guid id, CancellationToken ct) =>
        db.AccidentReports
            .Include(r => r.Offers).ThenInclude(o => o.PriceItems)
            .Include(r => r.Matches)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<PagedResult<ReportListItemResponse>> ListByCustomerAsync(Guid customerId, IPagedQuery paging, CancellationToken ct)
    {
        var query =
            from r in db.AccidentReports.AsNoTracking()
            join v in db.VehicleTypes on r.VehicleTypeId equals v.Id
            where r.CustomerId == customerId
            orderby r.CreatedAt descending
            select new ReportListItemResponse(
                r.Id,
                r.Status,
                r.City,
                r.District,
                v.Name,
                r.Offers.Count(o => o.Status != OfferStatus.Withdrawn),
                r.Offers.Where(o => o.Status != OfferStatus.Withdrawn).Min(o => (decimal?)o.TotalPrice),
                r.CreatedAt,
                r.ExpiresAt);

        return query.ToPagedResultAsync(paging, ct);
    }

    public Task<int> CountOpenByCustomerAsync(Guid customerId, CancellationToken ct) =>
        db.AccidentReports.CountAsync(r => r.CustomerId == customerId && r.Status == ReportStatus.Open, ct);

    public async Task<IReadOnlyList<AccidentReport>> ListExpiredOpenAsync(DateTimeOffset now, int batchSize, CancellationToken ct) =>
        await db.AccidentReports
            .Where(r => r.Status == ReportStatus.Open && r.ExpiresAt <= now)
            .OrderBy(r => r.ExpiresAt)
            .Take(batchSize)
            .Include(r => r.Offers)
            .AsSplitQuery()
            .ToListAsync(ct);
}
