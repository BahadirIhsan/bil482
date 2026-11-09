using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Common;

namespace IndustryApp.Application.Accounts;

public sealed class AccountService(IIdentityService identity, ICurrentUser currentUser) : IAccountService
{
    public async Task<Result<MeResponse>> GetMeAsync(CancellationToken ct)
    {
        var claims = await identity.GetClaimsAsync(currentUser.UserId, ct);
        var contact = claims is null ? null : await identity.GetContactAsync(currentUser.UserId, ct);
        if (claims is null || contact is null)
            return Errors.Auth.TokenExpired; // hesap pasifleştirilmiş veya silinmiş

        return new MeResponse(claims.UserId, claims.FullName, claims.Email, contact.Phone, claims.Roles, claims.MechanicStatus);
    }
}
