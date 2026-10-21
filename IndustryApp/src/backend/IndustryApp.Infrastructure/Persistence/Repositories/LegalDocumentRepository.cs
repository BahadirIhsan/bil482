using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class LegalDocumentRepository(AppDbContext db) : ILegalDocumentRepository
{
    public async Task<IReadOnlyList<LegalDocument>> ListCurrentForRoleAsync(string role, CancellationToken ct) =>
        await db.LegalDocuments.AsNoTracking()
            .Where(d => d.IsCurrent && (d.TargetRole == role || d.TargetRole == LegalDocument.AllRoles))
            .OrderBy(d => d.Code)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<LegalDocument>> GetCurrentByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken ct) =>
        await db.LegalDocuments.AsNoTracking()
            .Where(d => d.IsCurrent && codes.Contains(d.Code))
            .ToListAsync(ct);
}
