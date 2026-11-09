using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Common;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Common;
using IndustryApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndustryApp.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity'yi IIdentityService portunun arkasına alır. UserManager yalnızca burada kullanılır;
/// Identity hataları Error nesnesine çevrilir (DuplicateEmail → EMAIL_ALREADY_USED). 5 hatalı denemede hesap
/// 15 dakika kilitlenir (Lockout ayarları DependencyInjection'da).
/// </summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> users,
    AppDbContext db,
    IDateTimeProvider clock) : IIdentityService
{
    public async Task<Result<Guid>> CreateUserAsync(ICreateIdentityUserRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            PhoneNumber = request.Phone.Trim(),
            FullName = request.FullName.Trim(),
            IsActive = true,
            CreatedAt = clock.UtcNow
        };

        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return ToError(created);

        var role = await users.AddToRoleAsync(user, request.Role);
        if (!role.Succeeded)
            return Errors.Auth.UnknownRole;

        return user.Id;
    }

    public async Task<Result<IUserTokenClaims>> ValidateCredentialsAsync(ILoginRequest request, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
            return Errors.Auth.InvalidCredentials; // hesabın varlığı açığa çıkmaz

        if (await users.IsLockedOutAsync(user))
            return Errors.Auth.AccountLocked;

        if (!await users.CheckPasswordAsync(user, request.Password))
        {
            await users.AccessFailedAsync(user);
            return await users.IsLockedOutAsync(user) ? Errors.Auth.AccountLocked : Errors.Auth.InvalidCredentials;
        }

        await users.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = clock.UtcNow;
        await users.UpdateAsync(user);

        return Result.Success(await ClaimsOfAsync(user, ct));
    }

    public async Task<IUserTokenClaims?> GetClaimsAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        return user is { IsActive: true } ? await ClaimsOfAsync(user, ct) : null;
    }

    public async Task<IUserContact?> GetContactAsync(Guid userId, CancellationToken ct) =>
        await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserContact(u.Id, u.FullName, u.PhoneNumber ?? "", u.Email ?? ""))
            .FirstOrDefaultAsync(ct);

    private async Task<IUserTokenClaims> ClaimsOfAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = (await users.GetRolesAsync(user)).ToList();

        string? mechanicStatus = null;
        if (roles.Contains(RoleNames.Mechanic))
        {
            var status = await db.MechanicProfiles.AsNoTracking()
                .Where(m => m.Id == user.Id)
                .Select(m => (MechanicApprovalStatus?)m.ApprovalStatus)
                .FirstOrDefaultAsync(ct);
            mechanicStatus = status?.ToString();
        }

        return new UserTokenClaims(user.Id, user.Email ?? "", user.FullName, roles, mechanicStatus);
    }

    private static Error ToError(IdentityResult result)
    {
        var codes = result.Errors.Select(e => e.Code).ToList();
        if (codes.Any(c => c is "DuplicateEmail" or "DuplicateUserName"))
            return Errors.Auth.EmailAlreadyUsed;
        if (codes.Any(c => c.StartsWith("Password", StringComparison.Ordinal)))
            return Error.Validation(ErrorCodes.ValidationFailed, "Şifre en az 8 karakter olmalı, harf ve rakam içermelidir.");
        if (codes.Any(c => c is "InvalidEmail" or "InvalidUserName"))
            return Error.Validation(ErrorCodes.ValidationFailed, "Geçerli bir e-posta adresi girin.");
        return Error.Validation(ErrorCodes.ValidationFailed, "Hesap oluşturulamadı.");
    }

    private sealed record UserContact(Guid UserId, string FullName, string Phone, string Email) : IUserContact;
}
