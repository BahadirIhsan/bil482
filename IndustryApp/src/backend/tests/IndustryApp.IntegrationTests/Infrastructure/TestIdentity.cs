using System.Collections.Concurrent;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Common;

namespace IndustryApp.IntegrationTests.Infrastructure;

/// <summary>Testte isteği yapan kullanıcı; her DI kapsamında ayrı ayarlanır.</summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public Guid UserId { get; set; }

    public bool IsAuthenticated => UserId != Guid.Empty;

    public bool IsInRole(string role) => true;

    public string? IpAddress => "127.0.0.1";
}

/// <summary>
/// IIdentityService'in test sürümü: UC-04 testleri yalnızca GetContactAsync kullanır. Gerçek sürüm
/// (UserManager sarmalayıcı) kimlik altyapısıyla birlikte gelir.
/// </summary>
public sealed class TestIdentityService : IIdentityService
{
    private readonly ConcurrentDictionary<Guid, IUserContact> _contacts = new();

    public void Add(Guid userId, string fullName, string phone, string email) =>
        _contacts[userId] = new Contact(userId, fullName, phone, email);

    public Task<IUserContact?> GetContactAsync(Guid userId, CancellationToken ct) =>
        Task.FromResult(_contacts.GetValueOrDefault(userId));

    public Task<Result<Guid>> CreateUserAsync(ICreateIdentityUserRequest request, CancellationToken ct) =>
        throw new NotSupportedException();

    public Task<Result<IUserTokenClaims>> ValidateCredentialsAsync(ILoginRequest request, CancellationToken ct) =>
        throw new NotSupportedException();

    public Task<IUserTokenClaims?> GetClaimsAsync(Guid userId, CancellationToken ct) =>
        throw new NotSupportedException();

    private sealed record Contact(Guid UserId, string FullName, string Phone, string Email) : IUserContact;
}
