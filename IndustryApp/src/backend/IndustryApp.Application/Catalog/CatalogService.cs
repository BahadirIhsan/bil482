using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Catalog.Contracts;
using IndustryApp.Application.Common.Options;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Catalog;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IndustryApp.Application.Catalog;

public sealed class CatalogService(
    IDamageCategoryRepository categories,
    IVehicleTypeRepository vehicleTypes,
    ILegalDocumentRepository legalDocuments,
    IMemoryCache cache,
    IOptions<CatalogOptions> options) : ICatalogService
{
    private TimeSpan CacheDuration => TimeSpan.FromMinutes(options.Value.CacheMinutes);

    public async Task<IReadOnlyList<DamageCategoryNodeResponse>> GetDamageTreeAsync(CancellationToken ct) =>
        await cache.GetOrCreateAsync(CatalogCacheKeys.DamageTree, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            IReadOnlyList<DamageCategoryNodeResponse> tree =
                (await categories.GetTreeAsync(includeInactive: false, ct)).Select(ToNode).ToList();
            return tree;
        }) ?? [];

    public async Task<IReadOnlyList<VehicleTypeResponse>> GetVehicleTypesAsync(CancellationToken ct) =>
        await cache.GetOrCreateAsync(CatalogCacheKeys.VehicleTypes, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            IReadOnlyList<VehicleTypeResponse> list = (await vehicleTypes.ListActiveAsync(ct))
                .Select(v => new VehicleTypeResponse(v.Id, v.Code, v.Name))
                .ToList();
            return list;
        }) ?? [];

    public async Task<IReadOnlyList<LegalDocumentResponse>> GetLegalDocumentsAsync(string? role, CancellationToken ct)
    {
        var known = RoleNames.All.FirstOrDefault(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        if (known is null || known == RoleNames.Admin)
            return [];

        return (await legalDocuments.ListCurrentForRoleAsync(known, ct))
            .Select(d => new LegalDocumentResponse(d.Id, d.Code, d.Version, d.Title, d.Body))
            .ToList();
    }

    // Composite (PAT-02.2): dal ve yaprak aynı şekilde dolaşılır.
    private static DamageCategoryNodeResponse ToNode(DamageCategory category) =>
        new(category.Id, category.Code, category.Name, category.Level,
            category.SubCategories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).Select(ToNode).ToList());
}
