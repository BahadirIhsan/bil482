using FluentAssertions;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Accounts.Validators;
using IndustryApp.Domain.Accounts;

namespace IndustryApp.UnitTests.Application.Accounts;

/// <summary>Kayıt formu kuralları (T-8.4); frontend'deki Zod şemaları bu kurallarla aynıdır.</summary>
public sealed class RegisterValidatorTests
{
    [Fact]
    public void Customer_ValidRequest_Passes() =>
        new RegisterCustomerValidator().Validate(Customer()).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("5321234567")]
    [InlineData("0532123456")]
    [InlineData("+905321234567")]
    public void Customer_InvalidPhone_Fails(string phone) =>
        new RegisterCustomerValidator().Validate(Customer() with { Phone = phone })
            .Errors.Should().ContainSingle(e => e.PropertyName == nameof(IRegisterUserRequest.Phone));

    [Theory]
    [InlineData("kisa1")]
    [InlineData("sadeceharf")]
    [InlineData("12345678")]
    public void Customer_WeakPassword_Fails(string password) =>
        new RegisterCustomerValidator().Validate(Customer() with { Password = password })
            .Errors.Should().Contain(e => e.PropertyName == nameof(IRegisterUserRequest.Password));

    [Fact]
    public void Customer_NoConsent_Fails() =>
        new RegisterCustomerValidator().Validate(Customer() with { AcceptedLegalDocumentIds = [] })
            .IsValid.Should().BeFalse();

    [Theory]
    [InlineData("123456789", 39.9, 32.8, false)]
    [InlineData("1234567890", 91, 32.8, false)]
    [InlineData("1234567890", 39.9, 32.8, true)]
    public void Mechanic_TaxNumberLocationAndSpecialty_AreChecked(string taxNumber, double latitude, double longitude, bool emptySpecialties)
    {
        var request = new Mechanic("Usta", "usta@test.local", "05441234567", "parola123", "Atölye", taxNumber,
            "Ankara", "Ostim", "Ostim OSB", latitude, longitude, emptySpecialties ? [] : [1], [1, 3]);

        new RegisterMechanicValidator().Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Mechanic_ValidRequest_Passes() =>
        new RegisterMechanicValidator().Validate(new Mechanic("Usta", "usta@test.local", "05441234567", "parola123", "Atölye",
            "12345678901", "Ankara", "Ostim", "Ostim OSB", 39.97, 32.75, [1], [1, 3])).IsValid.Should().BeTrue();

    private static CustomerBody Customer() =>
        new("Test Müşteri", "musteri@test.local", "05321234567", "parola123", "Ankara", null, [1, 2]);

    private sealed record CustomerBody(
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
}
