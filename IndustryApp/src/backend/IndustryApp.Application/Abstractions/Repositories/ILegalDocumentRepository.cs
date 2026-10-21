using IndustryApp.Domain.Accounts;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface ILegalDocumentRepository
{
    /// <summary>Rolün güncel sözleşmeleri (rolün kendisine ve "All" hedefli olanlar).</summary>
    Task<IReadOnlyList<LegalDocument>> ListCurrentForRoleAsync(string role, CancellationToken ct);

    Task<IReadOnlyList<LegalDocument>> GetCurrentByCodesAsync(IReadOnlyCollection<string> codes, CancellationToken ct);
}
