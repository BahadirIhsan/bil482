namespace IndustryApp.Application.Accounts.Contracts;

/// <summary>
/// Kaydın rol bağımsız taban sözleşmesi (endpoint 1, 2). Role istemciden gelmez; WebApi'deki record'lar
/// kendi rolünü sabit döndürür. Hangi profilin üretileceğine IUserProfileFactory karar verir (PAT-01.1).
/// </summary>
public interface IRegisterUserRequest
{
    string Role { get; }

    string FullName { get; }

    string Email { get; }

    string Phone { get; }

    string Password { get; }

    IReadOnlyCollection<int> AcceptedLegalDocumentIds { get; }
}

/// <summary>Müşteri kaydı (endpoint 1).</summary>
public interface IRegisterCustomerRequest : IRegisterUserRequest
{
    string City { get; }

    string? District { get; }
}

/// <summary>Usta kaydı (endpoint 2). Hesap PendingApproval durumunda açılır.</summary>
public interface IRegisterMechanicRequest : IRegisterUserRequest
{
    string WorkshopName { get; }

    string TaxNumber { get; }

    string City { get; }

    string District { get; }

    string Address { get; }

    double Latitude { get; }

    double Longitude { get; }

    IReadOnlyCollection<int> SpecialtyCategoryIds { get; }
}

/// <summary>Kayıt sonucu. Usta hesabı admin onayına kadar eşleştirmeye girmez (FR-01.3).</summary>
public sealed record RegistrationResponse(Guid UserId, string Role, bool RequiresApproval);
