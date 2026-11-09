using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Common;
using IndustryApp.Domain.Accounts;

namespace IndustryApp.Application.Accounts.Factories;

/// <summary>Müşteri profili üretir (PAT-01.1). Zorunlu sözleşmeler: kullanım koşulları + müşteri KVKK metni.</summary>
public sealed class CustomerProfileFactory(ICustomerProfileRepository profiles, IDateTimeProvider clock) : IUserProfileFactory
{
    public string Role => RoleNames.Customer;

    public IReadOnlyCollection<string> RequiredLegalDocumentCodes { get; } = ["TERMS_OF_USE", "CUSTOMER_KVKK_NOTICE"];

    public async Task<Result> CreateProfileAsync(Guid userId, IRegisterUserRequest request, CancellationToken ct)
    {
        if (request is not IRegisterCustomerRequest customer)
            return Errors.Auth.UnknownRole;

        await profiles.AddAsync(CustomerProfile.Create(userId, customer.City, customer.District, clock.UtcNow), ct);
        return Result.Success();
    }
}
