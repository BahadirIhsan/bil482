using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Catalog.Contracts;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Catalog;

namespace IndustryApp.Application.Catalog;

public sealed class CatalogService(IDamageCategoryRepository categories, ILegalDocumentRepository legalDocuments) : ICatalogService
{
    public async Task<IReadOnlyList<DamageCategoryNodeResponse>> GetDamageTreeAsync(CancellationToken ct) =>
        (await categories.GetTreeAsync(includeInactive: false, ct)).Select(ToNode).ToList();

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
