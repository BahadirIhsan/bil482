using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Accounts.Contracts;
using IndustryApp.Application.Common;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Accounts.Factories;

/// <summary>
/// Usta profili üretir (PAT-01.1): vergi numarası tekil olmalı, uzmanlıklar 1. seviye aktif kategori olmalı.
/// Profil PendingApproval açılır; admin onayına kadar eşleştirmeye girmez (FR-01.3).
/// </summary>
public sealed class MechanicProfileFactory(
    IMechanicProfileRepository profiles,
    IDamageCategoryRepository categories,
    IDateTimeProvider clock) : IUserProfileFactory
{
    public string Role => RoleNames.Mechanic;

    public IReadOnlyCollection<string> RequiredLegalDocumentCodes { get; } = ["TERMS_OF_USE", "MECHANIC_SERVICE_AGREEMENT"];

    public async Task<Result> CreateProfileAsync(Guid userId, IRegisterUserRequest request, CancellationToken ct)
    {
        if (request is not IRegisterMechanicRequest mechanic)
            return Errors.Auth.UnknownRole;

        if (await profiles.ExistsByTaxNumberAsync(mechanic.TaxNumber.Trim(), ct))
            return Errors.Auth.TaxNumberAlreadyUsed;

        var specialtyIds = mechanic.SpecialtyCategoryIds.Distinct().ToList();
        var found = await categories.GetByIdsAsync(specialtyIds, ct);
        if (found.Count != specialtyIds.Count || found.Any(c => !c.IsRoot || !c.IsActive))
            return Errors.Auth.InvalidSpecialty;

        var profile = MechanicProfile.Create(
            userId,
            mechanic.WorkshopName,
            mechanic.TaxNumber,
            mechanic.City,
            mechanic.District,
            mechanic.Address,
            new GeoLocation(mechanic.Latitude, mechanic.Longitude),
            specialtyIds,
            clock.UtcNow);

        await profiles.AddAsync(profile, ct);
        return Result.Success();
    }
}
