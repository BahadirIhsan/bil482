using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Accounts;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Accounts.Factories;
using IndustryApp.Application.Common.Options;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Common;
using IndustryApp.Infrastructure.Identity;
using IndustryApp.Infrastructure.Persistence;
using IndustryApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IndustryApp.IntegrationTests.Uc01;

/// <summary>
/// UC-01 gerçek PostgreSQL + ASP.NET Core Identity üzerinde: kayıt (PAT-01.1) kullanıcı, rol, profil ve sözleşme
/// onaylarını tek transaction'da yazar; profil adımı başarısızsa kullanıcı da geri alınır. Giriş ve refresh token
/// rotasyonu gerçek JwtTokenService ile çalışır. Fixture'daki test IIdentityService yerine gerçek IdentityService kullanılır.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class RegistrationIntegrationTests(PostgresFixture fixture)
{
    private static readonly IOptions<JwtOptions> Jwt = Options.Create(new JwtOptions
    {
        Issuer = "industryapp",
        Audience = "industryapp-clients",
        SigningKey = "entegrasyon-testi-icin-en-az-32-bayt-uzunlugunda-anahtar",
        AccessTokenMinutes = 30,
        RefreshTokenDays = 14
    });

    [Fact]
    public async Task RegisterCustomer_ThenLoginAndRefresh_PersistsEverythingAndRotatesToken()
    {
        var email = $"musteri-{Guid.NewGuid():N}@test.local";
        RegistrationResponse registration;
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var docs = await LegalDocumentIdsAsync(scope, RoleNames.Customer);
            var result = await CreateAuthService(scope).RegisterAsync(
                new Customer("Kayıt Testi", email, "05321112233", "parola123", "Ankara", "Çankaya", docs), CancellationToken.None);
            result.IsSuccess.Should().BeTrue(result.Error?.Message);
            registration = result.Value;
        }

        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Users.SingleAsync(u => u.Id == registration.UserId)).Email.Should().Be(email);
            (await db.CustomerProfiles.SingleAsync(p => p.Id == registration.UserId)).City.Should().Be("Ankara");
            (await db.UserConsents.CountAsync(c => c.UserId == registration.UserId)).Should().Be(2);
        }

        AuthResponse login;
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var result = await CreateAuthService(scope).LoginAsync(new Login(email, "parola123"), CancellationToken.None);
            result.IsSuccess.Should().BeTrue(result.Error?.Message);
            login = result.Value;
        }

        var token = new JwtSecurityTokenHandler().ReadJwtToken(login.AccessToken);
        token.Subject.Should().Be(registration.UserId.ToString());
        token.Claims.Where(c => c.Type == "role").Select(c => c.Value).Should().Equal(RoleNames.Customer);

        AuthResponse refreshed;
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var result = await CreateAuthService(scope).RefreshAsync(new Refresh(login.RefreshToken), CancellationToken.None);
            result.IsSuccess.Should().BeTrue(result.Error?.Message);
            refreshed = result.Value;
        }

        refreshed.RefreshToken.Should().NotBe(login.RefreshToken);

        // Eski (rotasyonla kapanmış) token tekrar gelirse: reddedilir ve yenisi de dahil tüm oturumlar kapanır.
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var reuse = await CreateAuthService(scope).RefreshAsync(new Refresh(login.RefreshToken), CancellationToken.None);
            reuse.Error!.Code.Should().Be(ErrorCodes.TokenExpired);
        }

        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var afterTheft = await CreateAuthService(scope).RefreshAsync(new Refresh(refreshed.RefreshToken), CancellationToken.None);
            afterTheft.Error!.Code.Should().Be(ErrorCodes.TokenExpired);
        }
    }

    [Fact]
    public async Task RegisterMechanic_DuplicateTaxNumber_RollsBackIdentityUser()
    {
        var taxNumber = Random.Shared.NextInt64(1_000_000_000, 9_999_999_999).ToString();
        var firstEmail = $"usta-{Guid.NewGuid():N}@test.local";
        var secondEmail = $"usta-{Guid.NewGuid():N}@test.local";

        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var first = await CreateAuthService(scope).RegisterAsync(await MechanicAsync(scope, firstEmail, taxNumber), CancellationToken.None);
            first.IsSuccess.Should().BeTrue(first.Error?.Message);
            first.Value.RequiresApproval.Should().BeTrue();
        }

        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var second = await CreateAuthService(scope).RegisterAsync(await MechanicAsync(scope, secondEmail, taxNumber), CancellationToken.None);
            second.Error!.Code.Should().Be(ErrorCodes.TaxNumberAlreadyUsed);
        }

        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Users.AnyAsync(u => u.Email == firstEmail)).Should().BeTrue();
            (await db.Users.AnyAsync(u => u.Email == secondEmail)).Should().BeFalse("profil adımı başarısız olunca kullanıcı da geri alınmalı");
        }
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsEmailAlreadyUsed()
    {
        var email = $"tekrar-{Guid.NewGuid():N}@test.local";
        for (var attempt = 0; attempt < 2; attempt++)
        {
            await using var scope = fixture.Services.CreateAsyncScope();
            var docs = await LegalDocumentIdsAsync(scope, RoleNames.Customer);
            var result = await CreateAuthService(scope).RegisterAsync(
                new Customer("Tekrar", email.ToUpperInvariant(), "05321112233", "parola123", "Ankara", null, docs), CancellationToken.None);

            if (attempt == 0)
                result.IsSuccess.Should().BeTrue(result.Error?.Message);
            else
                result.Error!.Code.Should().Be(ErrorCodes.EmailAlreadyUsed);
        }
    }

    private static AuthService CreateAuthService(AsyncServiceScope scope)
    {
        var sp = scope.ServiceProvider;
        var clock = sp.GetRequiredService<IDateTimeProvider>();
        return new AuthService(
            sp.GetServices<IUserProfileFactory>(),
            sp.GetRequiredService<ILegalDocumentRepository>(),
            sp.GetRequiredService<IUserConsentRepository>(),
            ActivatorUtilities.CreateInstance<IdentityService>(sp),
            new JwtTokenService(Jwt, clock),
            sp.GetRequiredService<IRefreshTokenRepository>(),
            sp.GetRequiredService<IUnitOfWork>(),
            sp.GetRequiredService<ICurrentUser>(),
            clock);
    }

    private static async Task<IReadOnlyCollection<int>> LegalDocumentIdsAsync(AsyncServiceScope scope, string role) =>
        (await scope.ServiceProvider.GetRequiredService<ILegalDocumentRepository>().ListCurrentForRoleAsync(role, CancellationToken.None))
            .Select(d => d.Id).ToList();

    private static async Task<Mechanic> MechanicAsync(AsyncServiceScope scope, string email, string taxNumber)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rootCategory = await db.DamageCategories.Where(c => c.ParentId == null && c.IsActive).Select(c => c.Id).FirstAsync();
        return new Mechanic("Usta Test", email, "05441112233", "parola123", "Atölye Test", taxNumber,
            "Ankara", "Ostim", "Ostim OSB", 39.97, 32.75, [rootCategory], await LegalDocumentIdsAsync(scope, RoleNames.Mechanic));
    }

    private sealed record Customer(
        string FullName, string Email, string Phone, string Password, string City, string? District,
        IReadOnlyCollection<int> AcceptedLegalDocumentIds) : IRegisterCustomerRequest
    {
        public string Role => RoleNames.Customer;
    }

    private sealed record Mechanic(
        string FullName, string Email, string Phone, string Password, string WorkshopName, string TaxNumber,
        string City, string District, string Address, double Latitude, double Longitude,
        IReadOnlyCollection<int> SpecialtyCategoryIds, IReadOnlyCollection<int> AcceptedLegalDocumentIds) : IRegisterMechanicRequest
    {
        public string Role => RoleNames.Mechanic;
    }

    private sealed record Login(string Email, string Password) : ILoginRequest;

    private sealed record Refresh(string RefreshToken) : IRefreshTokenRequest;
}
