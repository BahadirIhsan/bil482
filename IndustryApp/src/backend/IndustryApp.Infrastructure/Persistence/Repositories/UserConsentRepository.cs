using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Domain.Accounts;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class UserConsentRepository(AppDbContext db) : IUserConsentRepository
{
    public Task AddRangeAsync(IEnumerable<UserConsent> consents, CancellationToken ct) =>
        db.UserConsents.AddRangeAsync(consents, ct);
}
