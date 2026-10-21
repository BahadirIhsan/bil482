using IndustryApp.Domain.Catalog;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface IDamageCategoryRepository
{
    /// <summary>Kök kategorileri alt ağaçlarıyla (Composite) döner, sıra numarasına göre sıralı.</summary>
    Task<IReadOnlyList<DamageCategory>> GetTreeAsync(bool includeInactive, CancellationToken ct);

    Task<IReadOnlyList<DamageCategory>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct);

    Task<DamageCategory?> GetAsync(int id, CancellationToken ct);

    Task AddAsync(DamageCategory category, CancellationToken ct);
}
