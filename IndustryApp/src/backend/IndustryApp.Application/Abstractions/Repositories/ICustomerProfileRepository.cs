using IndustryApp.Domain.Accounts;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface ICustomerProfileRepository
{
    Task<CustomerProfile?> GetAsync(Guid userId, CancellationToken ct);

    Task AddAsync(CustomerProfile profile, CancellationToken ct);
}
