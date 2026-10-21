using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class VehicleTypeRepository(AppDbContext db) : IVehicleTypeRepository
{
    public async Task<IReadOnlyList<VehicleType>> ListActiveAsync(CancellationToken ct) =>
        await db.VehicleTypes.AsNoTracking()
            .Where(v => v.IsActive)
            .OrderBy(v => v.SortOrder).ThenBy(v => v.Name)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(short id, CancellationToken ct) =>
        db.VehicleTypes.AnyAsync(v => v.Id == id && v.IsActive, ct);
}
