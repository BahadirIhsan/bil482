using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class DamageCategoryRepository(AppDbContext db) : IDamageCategoryRepository
{
    public async Task<IReadOnlyList<DamageCategory>> GetTreeAsync(bool includeInactive, CancellationToken ct)
    {
        // Tüm düğümler tek sorguda yüklenir; EF ilişki düzeltmesi (fix-up) alt kategori listelerini doldurur.
        var all = await db.DamageCategories
            .Where(c => includeInactive || c.IsActive)
            .OrderBy(c => c.Level).ThenBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);

        return all.Where(c => c.ParentId == null).ToList();
    }

    public async Task<IReadOnlyList<DamageCategory>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct) =>
        await db.DamageCategories.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

    public async Task<DamageCategory?> GetAsync(int id, CancellationToken ct) =>
        await db.DamageCategories.FindAsync([id], ct);

    public async Task AddAsync(DamageCategory category, CancellationToken ct) =>
        await db.DamageCategories.AddAsync(category, ct);
}
