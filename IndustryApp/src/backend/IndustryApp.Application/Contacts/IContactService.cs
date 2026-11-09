using IndustryApp.Application.Common;
using IndustryApp.Application.Contacts.Contracts;

namespace IndustryApp.Application.Contacts;

/// <summary>Eşleşme sonrası karşı tarafın iletişim bilgisi (FR-04.4).</summary>
public interface IContactService
{
    Task<Result<MatchContactResponse>> GetMatchContactAsync(Guid reportId, CancellationToken ct);
}
