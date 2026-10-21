using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Admin.Contracts;
using IndustryApp.Application.Common;
using IndustryApp.Application.Matching;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Offers;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class MechanicProfileRepository(AppDbContext db) : IMechanicProfileRepository
{
    private const double EarthRadiusKm = 6371.0;
    private const double DegreesToRadians = Math.PI / 180.0;

    public Task<MechanicProfile?> GetAsync(Guid userId, CancellationToken ct) =>
        db.MechanicProfiles.Include(m => m.Specialties).FirstOrDefaultAsync(m => m.Id == userId, ct);

    public async Task AddAsync(MechanicProfile profile, CancellationToken ct) =>
        await db.MechanicProfiles.AddAsync(profile, ct);

    public Task<bool> ExistsByTaxNumberAsync(string taxNumber, CancellationToken ct) =>
        db.MechanicProfiles.AnyAsync(m => m.TaxNumber == taxNumber, ct);

    public Task<PagedResult<PendingMechanicResponse>> ListByApprovalStatusAsync(
        MechanicApprovalStatus? status, IPagedQuery paging, CancellationToken ct)
    {
        var query =
            from m in db.MechanicProfiles.AsNoTracking()
            join u in db.Users on m.Id equals u.Id
            where status == null || m.ApprovalStatus == status
            orderby m.CreatedAt
            select new PendingMechanicResponse(
                m.Id,
                u.FullName,
                u.Email!,
                m.WorkshopName,
                m.TaxNumber,
                m.City,
                m.District,
                m.Address,
                m.ApprovalStatus,
                m.RejectionReason,
                m.Specialties.Select(s => s.DamageCategoryId).ToList(),
                m.CreatedAt);

        return query.ToPagedResultAsync(paging, ct);
    }

    // Aday sorgusu yalnızca konum ve uzmanlıkla daraltır; onay, aktiflik, müsaitlik ve kapasite
    // kontrollerini filtre zinciri (PAT-03.3) yapar, böylece her filtrenin kaç adayı elediği görülebilir.
    public async Task<IReadOnlyList<MechanicCandidate>> FindCandidatesByCityAsync(
        string city, IReadOnlyCollection<int> rootCategoryIds, CancellationToken ct) =>
        await (
            from m in db.MechanicProfiles.AsNoTracking()
            join u in db.Users on m.Id equals u.Id
            where m.City == city && m.Specialties.Any(s => rootCategoryIds.Contains(s.DamageCategoryId))
            orderby m.CreatedAt
            select new MechanicCandidate(
                m.Id,
                m.City,
                m.Latitude,
                m.Longitude,
                m.ApprovalStatus,
                u.IsActive,
                m.IsAvailable,
                m.DailyCapacity,
                m.Specialties.Select(s => s.DamageCategoryId).ToList(),
                null))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MechanicCandidate>> FindCandidatesWithinRadiusAsync(
        double latitude, double longitude, double radiusKm, IReadOnlyCollection<int> rootCategoryIds, CancellationToken ct)
    {
        // Haversine; PostGIS gerekmez (T-13.1).
        var lat1 = latitude * DegreesToRadians;
        var lon1 = longitude * DegreesToRadians;
        var cosLat1 = Math.Cos(lat1);

        var query =
            from m in db.MechanicProfiles.AsNoTracking()
            join u in db.Users on m.Id equals u.Id
            where m.Specialties.Any(s => rootCategoryIds.Contains(s.DamageCategoryId))
            let halfDLat = (m.Latitude * DegreesToRadians - lat1) / 2
            let halfDLon = (m.Longitude * DegreesToRadians - lon1) / 2
            let a = Math.Sin(halfDLat) * Math.Sin(halfDLat)
                    + cosLat1 * Math.Cos(m.Latitude * DegreesToRadians) * Math.Sin(halfDLon) * Math.Sin(halfDLon)
            let distanceKm = 2 * EarthRadiusKm * Math.Asin(Math.Sqrt(a))
            where distanceKm <= radiusKm
            orderby distanceKm
            select new MechanicCandidate(
                m.Id,
                m.City,
                m.Latitude,
                m.Longitude,
                m.ApprovalStatus,
                u.IsActive,
                m.IsAvailable,
                m.DailyCapacity,
                m.Specialties.Select(s => s.DamageCategoryId).ToList(),
                distanceKm);

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountAcceptedTodayAsync(
        IReadOnlyCollection<Guid> mechanicIds, DateTimeOffset dayStart, CancellationToken ct)
    {
        var dayEnd = dayStart.AddDays(1);
        return await db.Offers.AsNoTracking()
            .Where(o => mechanicIds.Contains(o.MechanicId)
                        && o.Status == OfferStatus.Accepted
                        && o.DecidedAt >= dayStart
                        && o.DecidedAt < dayEnd)
            .GroupBy(o => o.MechanicId)
            .Select(g => new { MechanicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MechanicId, x => x.Count, ct);
    }
}
