using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Offers.Contracts;
using IndustryApp.Application.Offers.Sorting;
using IndustryApp.Domain.Offers;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class OfferRepository(AppDbContext db) : IOfferRepository
{
    public async Task AddAsync(Offer offer, CancellationToken ct) =>
        await db.Offers.AddAsync(offer, ct);

    public Task<Offer?> GetAsync(Guid id, CancellationToken ct) =>
        db.Offers.Include(o => o.PriceItems).FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Offer?> GetByReportAndMechanicAsync(Guid reportId, Guid mechanicId, CancellationToken ct) =>
        db.Offers.Include(o => o.PriceItems).FirstOrDefaultAsync(o => o.ReportId == reportId && o.MechanicId == mechanicId, ct);

    public async Task<IReadOnlyList<OfferListItemResponse>> ListForReportAsync(Guid reportId, IOfferSortStrategy sort, CancellationToken ct)
    {
        var visible = db.Offers.AsNoTracking().Where(o =>
            o.ReportId == reportId
            && (o.Status == OfferStatus.Pending || o.Status == OfferStatus.Accepted || o.Status == OfferStatus.NotSelected));

        // Sıralama dış sorguda kalsın diye atölye adı ilişkili alt sorguyla okunur.
        return await sort.Apply(visible)
            .Select(o => new OfferListItemResponse(
                o.Id,
                o.MechanicId,
                db.MechanicProfiles.Where(m => m.Id == o.MechanicId).Select(m => m.WorkshopName).First(),
                o.TotalPrice,
                o.Currency,
                o.EstimatedDays,
                o.Note,
                o.Status,
                o.CreatedAt,
                o.UpdatedAt,
                o.PriceItems.OrderBy(p => p.SortOrder)
                    .Select(p => new OfferPriceLineResponse(p.ItemType, p.Description, p.Amount))
                    .ToList()))
            .ToListAsync(ct);
    }

    public Task<PagedResult<MyOfferListItemResponse>> ListByMechanicAsync(
        Guid mechanicId, OfferStatus? status, IPagedQuery paging, CancellationToken ct)
    {
        var query =
            from o in db.Offers.AsNoTracking()
            join r in db.AccidentReports on o.ReportId equals r.Id
            where o.MechanicId == mechanicId && (status == null || o.Status == status)
            orderby o.CreatedAt descending
            select new MyOfferListItemResponse(
                o.Id, o.ReportId, r.City, r.District, o.TotalPrice, o.Currency, o.EstimatedDays,
                o.Status, r.Status, o.CreatedAt, o.UpdatedAt);

        return query.ToPagedResultAsync(paging, ct);
    }

    public async Task<IReadOnlyList<Offer>> ListByReportAsync(Guid reportId, CancellationToken ct) =>
        await db.Offers.Include(o => o.PriceItems).Where(o => o.ReportId == reportId).ToListAsync(ct);
}
