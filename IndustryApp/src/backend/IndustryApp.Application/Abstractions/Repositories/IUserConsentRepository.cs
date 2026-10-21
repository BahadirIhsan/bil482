using IndustryApp.Domain.Accounts;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface IUserConsentRepository
{
    Task AddRangeAsync(IEnumerable<UserConsent> consents, CancellationToken ct);
}
