using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Domain.Accounts;

namespace IndustryApp.WebApi.Contracts;

/// <summary>POST /api/auth/register/customer gövdesi. Rol istemciden alınmaz.</summary>
public sealed record RegisterCustomerRequest(
    string FullName,
    string Email,
    string Phone,
    string Password,
    string City,
    string? District,
    IReadOnlyCollection<int> AcceptedLegalDocumentIds) : IRegisterCustomerRequest
{
    public string Role => RoleNames.Customer;
}

/// <summary>POST /api/auth/register/mechanic gövdesi. Hesap PendingApproval açılır.</summary>
public sealed record RegisterMechanicRequest(
    string FullName,
    string Email,
    string Phone,
    string Password,
    string WorkshopName,
    string TaxNumber,
    string City,
    string District,
    string Address,
    double Latitude,
    double Longitude,
    IReadOnlyCollection<int> SpecialtyCategoryIds,
    IReadOnlyCollection<int> AcceptedLegalDocumentIds) : IRegisterMechanicRequest
{
    public string Role => RoleNames.Mechanic;
}

public sealed record LoginRequest(string Email, string Password) : ILoginRequest;

public sealed record RefreshTokenRequest(string RefreshToken) : IRefreshTokenRequest;
