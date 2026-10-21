using IndustryApp.Application.Admin.Contracts;
using IndustryApp.Application.Common;
using IndustryApp.Application.Matching;
using IndustryApp.Domain.Accounts;

namespace IndustryApp.Application.Abstractions.Repositories;

public interface IMechanicProfileRepository
{
    /// <summary>Profili uzmanlıklarıyla birlikte yükler.</summary>
    Task<MechanicProfile?> GetAsync(Guid userId, CancellationToken ct);

    Task AddAsync(MechanicProfile profile, CancellationToken ct);

    Task<bool> ExistsByTaxNumberAsync(string taxNumber, CancellationToken ct);

    /// <summary>Admin onay listesi; status null ise tüm durumlar.</summary>
    Task<PagedResult<PendingMechanicResponse>> ListByApprovalStatusAsync(
        MechanicApprovalStatus? status, IPagedQuery paging, CancellationToken ct);

    /// <summary>Şehir stratejisi: aynı şehirde, uzmanlığı rapor kök kategorilerinden biriyle kesişen ustalar.</summary>
    Task<IReadOnlyList<MechanicCandidate>> FindCandidatesByCityAsync(
        string city, IReadOnlyCollection<int> rootCategoryIds, CancellationToken ct);

    /// <summary>Mesafe stratejisi: verilen yarıçap içindeki (Haversine) ve uzmanlığı kesişen ustalar, en yakından uzağa.</summary>
    Task<IReadOnlyList<MechanicCandidate>> FindCandidatesWithinRadiusAsync(
        double latitude, double longitude, double radiusKm, IReadOnlyCollection<int> rootCategoryIds, CancellationToken ct);

    /// <summary>Her ustanın [dayStart, dayStart + 1 gün) aralığında kabul edilen teklif sayısı (günlük kapasite filtresi).</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountAcceptedTodayAsync(
        IReadOnlyCollection<Guid> mechanicIds, DateTimeOffset dayStart, CancellationToken ct);
}
