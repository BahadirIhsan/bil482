using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Domain.Accounts;

namespace IndustryApp.Infrastructure.Persistence.Repositories;

public sealed class CustomerProfileRepository(AppDbContext db) : ICustomerProfileRepository
{
    public async Task<CustomerProfile?> GetAsync(Guid userId, CancellationToken ct) =>
        await db.CustomerProfiles.FindAsync([userId], ct);

    public async Task AddAsync(CustomerProfile profile, CancellationToken ct) =>
        await db.CustomerProfiles.AddAsync(profile, ct);
}
