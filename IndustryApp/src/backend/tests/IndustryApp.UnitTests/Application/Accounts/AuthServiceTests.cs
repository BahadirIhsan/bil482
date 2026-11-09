using FluentAssertions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Accounts;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Accounts.Factories;
using IndustryApp.Application.Common;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Catalog;
using IndustryApp.Domain.Common;
using NSubstitute;

namespace IndustryApp.UnitTests.Application.Accounts;

/// <summary>
/// UC-01 · Kayıt Factory Method'u (PAT-01.1), sözleşme onayı ve refresh token rotasyonu. Gerçek fabrikalar sahte
/// repository'lerle çalışır; Identity ve token servisi sahte.
/// </summary>
public sealed class AuthServiceTests
{
    private readonly ServiceTestContext _ctx = new();
    private readonly ILegalDocumentRepository _legal = Substitute.For<ILegalDocumentRepository>();
    private readonly IUserConsentRepository _consents = Substitute.For<IUserConsentRepository>();
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly ICustomerProfileRepository _customers = Substitute.For<ICustomerProfileRepository>();
    private readonly IMechanicProfileRepository _mechanics = Substitute.For<IMechanicProfileRepository>();
    private readonly IDamageCategoryRepository _categories = Substitute.For<IDamageCategoryRepository>();
    private readonly IUnitOfWorkTransaction _transaction = Substitute.For<IUnitOfWorkTransaction>();
    private readonly Guid _newUserId = Guid.NewGuid();
    private readonly AuthService _service;

    private static readonly LegalDocument Terms = TestEntities.LegalDocument(1, "TERMS_OF_USE");
    private static readonly LegalDocument CustomerKvkk = TestEntities.LegalDocument(2, "CUSTOMER_KVKK_NOTICE", RoleNames.Customer);
    private static readonly LegalDocument MechanicAgreement = TestEntities.LegalDocument(3, "MECHANIC_SERVICE_AGREEMENT", RoleNames.Mechanic);

    public AuthServiceTests()
    {
        _legal.GetCurrentByCodesAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(call => new[] { Terms, CustomerKvkk, MechanicAgreement }
                .Where(d => call.Arg<IReadOnlyCollection<string>>().Contains(d.Code)).ToList());
        _identity.CreateUserAsync(Arg.Any<ICreateIdentityUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(_newUserId));
        _ctx.UnitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        var front = DamageCategory.Create("BODY", "Kaporta", 1).WithId(10);
        _categories.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(call => new[] { front }.Where(c => call.Arg<IReadOnlyCollection<int>>().Contains(c.Id)).ToList());

        _tokens.CreateAccessToken(Arg.Any<IUserTokenClaims>()).Returns(new Access("access", _ctx.Now.AddMinutes(30)));
        _tokens.CreateRefreshToken().Returns(new Refresh("new-refresh", "NEW_HASH", _ctx.Now.AddDays(14)));
        _tokens.HashRefreshToken(Arg.Any<string>()).Returns(call => "HASH:" + call.Arg<string>());

        IUserProfileFactory[] factories =
        [
            new CustomerProfileFactory(_customers, _ctx.Clock),
            new MechanicProfileFactory(_mechanics, _categories, _ctx.Clock)
        ];
        _service = new AuthService(
            factories, _legal, _consents, _identity, _tokens, _refreshTokens, _ctx.UnitOfWork, _ctx.CurrentUser, _ctx.Clock);
    }

    [Fact]
    public async Task Register_Customer_CreatesCustomerProfile()
    {
        var result = await _service.RegisterAsync(Customer(accepted: [1, 2]), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new RegistrationResponse(_newUserId, RoleNames.Customer, RequiresApproval: false));
        await _identity.Received(1).CreateUserAsync(
            Arg.Is<ICreateIdentityUserRequest>(r => r.Role == RoleNames.Customer && r.Email == "musteri@test.local"), Arg.Any<CancellationToken>());
        await _customers.Received(1).AddAsync(Arg.Is<CustomerProfile>(p => p.UserId == _newUserId && p.City == "Ankara"), Arg.Any<CancellationToken>());
        await _mechanics.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _consents.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<UserConsent>>(c => c.Select(x => x.LegalDocumentId).Order().SequenceEqual(new[] { 1, 2 })), Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_Mechanic_CreatesPendingProfile()
    {
        var result = await _service.RegisterAsync(Mechanic(accepted: [1, 3], specialties: [10]), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RequiresApproval.Should().BeTrue();
        await _mechanics.Received(1).AddAsync(
            Arg.Is<MechanicProfile>(p => p.UserId == _newUserId && p.ApprovalStatus == MechanicApprovalStatus.PendingApproval),
            Arg.Any<CancellationToken>());
        await _customers.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Register_UnknownRole_ReturnsUnknownRole()
    {
        var request = Customer(accepted: [1, 2]) with { RoleOverride = "Pilot" };

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.UnknownRole);
        await _identity.DidNotReceiveWithAnyArgs().CreateUserAsync(default!, default);
    }

    [Fact]
    public async Task Register_MissingRequiredConsent_ReturnsConsentRequired()
    {
        var result = await _service.RegisterAsync(Customer(accepted: [1]), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.ConsentRequired);
        await _identity.DidNotReceiveWithAnyArgs().CreateUserAsync(default!, default);
    }

    [Fact]
    public async Task Register_Mechanic_DuplicateTaxNumber_RollsBackUser()
    {
        _mechanics.ExistsByTaxNumberAsync("1234567890", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.RegisterAsync(Mechanic(accepted: [1, 3], specialties: [10]), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.TaxNumberAlreadyUsed);
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).DisposeAsync(); // commit edilmeden kapanan transaction geri alınır
    }

    [Fact]
    public async Task Register_Mechanic_NonRootOrUnknownSpecialty_ReturnsInvalidSpecialty()
    {
        var result = await _service.RegisterAsync(Mechanic(accepted: [1, 3], specialties: [10, 99]), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.InvalidSpecialty);
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_ValidCredentials_IssuesAndStoresRefreshTokenHash()
    {
        var claims = new UserTokenClaims(Guid.NewGuid(), "a@b.c", "Ad Soyad", [RoleNames.Customer], null);
        _identity.ValidateCredentialsAsync(Arg.Any<ILoginRequest>(), Arg.Any<CancellationToken>()).Returns(Result.Success<IUserTokenClaims>(claims));

        var result = await _service.LoginAsync(new Login("a@b.c", "parola123"), CancellationToken.None);

        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("new-refresh");
        result.Value.User.Roles.Should().Equal(RoleNames.Customer);
        await _refreshTokens.Received(1).AddAsync(claims.UserId, "NEW_HASH", Arg.Any<DateTimeOffset>(), _ctx.Now, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Refresh_ActiveToken_RotatesAndRevokesOld()
    {
        var userId = Guid.NewGuid();
        var stored = new Stored(Guid.NewGuid(), userId, _ctx.Now.AddDays(1), null);
        _refreshTokens.GetByHashAsync("HASH:old", Arg.Any<CancellationToken>()).Returns(stored);
        _identity.GetClaimsAsync(userId, Arg.Any<CancellationToken>()).Returns(new UserTokenClaims(userId, "a@b.c", "A", [RoleNames.Customer], null));

        var result = await _service.RefreshAsync(new RefreshBody("old"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _refreshTokens.Received(1).RevokeAsync(stored.Id, _ctx.Now, "NEW_HASH", Arg.Any<CancellationToken>());
        await _refreshTokens.Received(1).AddAsync(userId, "NEW_HASH", Arg.Any<DateTimeOffset>(), _ctx.Now, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Refresh_RevokedTokenReused_RevokesAllUserTokens()
    {
        var userId = Guid.NewGuid();
        _refreshTokens.GetByHashAsync("HASH:stolen", Arg.Any<CancellationToken>())
            .Returns(new Stored(Guid.NewGuid(), userId, _ctx.Now.AddDays(1), _ctx.Now.AddHours(-1)));

        var result = await _service.RefreshAsync(new RefreshBody("stolen"), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.TokenExpired);
        await _refreshTokens.Received(1).RevokeAllActiveAsync(userId, _ctx.Now, Arg.Any<CancellationToken>());
        _tokens.DidNotReceive().CreateRefreshToken();
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ReturnsTokenExpired()
    {
        _refreshTokens.GetByHashAsync("HASH:old", Arg.Any<CancellationToken>())
            .Returns(new Stored(Guid.NewGuid(), Guid.NewGuid(), _ctx.Now.AddSeconds(-1), null));

        var result = await _service.RefreshAsync(new RefreshBody("old"), CancellationToken.None);

        result.Error!.Code.Should().Be(ErrorCodes.TokenExpired);
        _tokens.DidNotReceive().CreateAccessToken(Arg.Any<IUserTokenClaims>());
    }

    [Fact]
    public async Task Logout_OtherUsersToken_DoesNotRevoke()
    {
        _refreshTokens.GetByHashAsync("HASH:x", Arg.Any<CancellationToken>())
            .Returns(new Stored(Guid.NewGuid(), Guid.NewGuid(), _ctx.Now.AddDays(1), null));

        var result = await _service.LogoutAsync(new RefreshBody("x"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _refreshTokens.DidNotReceiveWithAnyArgs().RevokeAsync(default, default, default, default);
    }

    private static CustomerRequest Customer(int[] accepted) =>
        new("Test Müşteri", "musteri@test.local", "05321234567", "parola123", "Ankara", "Çankaya", accepted);

    private static MechanicRequest Mechanic(int[] accepted, int[] specialties) =>
        new("Test Usta", "usta1@test.local", "05441234567", "parola123", "Atölye A", "1234567890",
            "Ankara", "Ostim", "Ostim OSB", 39.97, 32.75, specialties, accepted);

    private sealed record CustomerRequest(
        string FullName, string Email, string Phone, string Password, string City, string? District,
        IReadOnlyCollection<int> AcceptedLegalDocumentIds, string? RoleOverride = null) : IRegisterCustomerRequest
    {
        public string Role => RoleOverride ?? RoleNames.Customer;
    }

    private sealed record MechanicRequest(
        string FullName, string Email, string Phone, string Password, string WorkshopName, string TaxNumber,
        string City, string District, string Address, double Latitude, double Longitude,
        IReadOnlyCollection<int> SpecialtyCategoryIds, IReadOnlyCollection<int> AcceptedLegalDocumentIds) : IRegisterMechanicRequest
    {
        public string Role => RoleNames.Mechanic;
    }

    private sealed record Login(string Email, string Password) : ILoginRequest;

    private sealed record RefreshBody(string RefreshToken) : IRefreshTokenRequest;

    private sealed record Access(string Token, DateTimeOffset ExpiresAt) : IAccessToken;

    private sealed record Refresh(string Token, string TokenHash, DateTimeOffset ExpiresAt) : IRefreshTokenPair;

    private sealed record Stored(Guid Id, Guid UserId, DateTimeOffset ExpiresAt, DateTimeOffset? RevokedAt) : IStoredRefreshToken;
}
