using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Reports.Contracts;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class ReportMatchRepository(AppDbContext db) : IReportMatchRepository
{
    public Task AddRangeAsync(IEnumerable<ReportMatch> matches, CancellationToken ct) =>
        db.ReportMatches.AddRangeAsync(matches, ct);

    public Task<bool> IsMatchedAsync(Guid reportId, Guid mechanicId, CancellationToken ct) =>
        db.ReportMatches.AnyAsync(m => m.ReportId == reportId && m.MechanicId == mechanicId, ct);

    public Task<PagedResult<MechanicJobListItemResponse>> ListJobsForMechanicAsync(
        Guid mechanicId, MechanicJobFilter filter, IPagedQuery paging, CancellationToken ct)
    {
        // Ustanın yalnızca kendi teklifi birleştirilir; başka ustaların teklifleri sorguya hiç girmez (K-06).
        var rows =
            from m in db.ReportMatches.AsNoTracking()
            join r in db.AccidentReports on m.ReportId equals r.Id
            join v in db.VehicleTypes on r.VehicleTypeId equals v.Id
            join o in db.Offers.Where(o => o.MechanicId == mechanicId) on m.ReportId equals o.ReportId into myOffers
            from my in myOffers.DefaultIfEmpty()
            where m.MechanicId == mechanicId
            select new { Match = m, Report = r, VehicleTypeName = v.Name, MyOffer = my };

        rows = filter switch
        {
            MechanicJobFilter.New => rows.Where(x => x.MyOffer == null && x.Report.Status == ReportStatus.Open),
            MechanicJobFilter.Offered => rows.Where(x => x.MyOffer != null && x.MyOffer.Status == OfferStatus.Pending),
            MechanicJobFilter.Won => rows.Where(x => x.MyOffer != null && x.MyOffer.Status == OfferStatus.Accepted),
            MechanicJobFilter.Lost => rows.Where(x => x.MyOffer != null && x.MyOffer.Status == OfferStatus.NotSelected),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, "Bilinmeyen iş filtresi.")
        };

        var query = rows
            .OrderByDescending(x => x.Match.NotifiedAt)
            .Select(x => new MechanicJobListItemResponse(
                x.Report.Id,
                x.Report.Status,
                x.Report.City,
                x.Report.District,
                x.VehicleTypeName,
                x.Report.DamageItems.Count,
                x.Match.NotifiedAt,
                x.Match.ViewedAt,
                x.Report.ExpiresAt,
                x.MyOffer != null ? x.MyOffer.Status : null,
                x.MyOffer != null ? x.MyOffer.TotalPrice : null));

        return query.ToPagedResultAsync(paging, ct);
    }

    public async Task<bool> MarkViewedAsync(Guid reportId, Guid mechanicId, DateTimeOffset now, CancellationToken ct)
    {
        var match = await db.ReportMatches.FirstOrDefaultAsync(m => m.ReportId == reportId && m.MechanicId == mechanicId, ct);
        if (match is null)
            return false;

        match.MarkViewed(now);
        return true;
    }
}
