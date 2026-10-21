using IndustryApp.Domain.Accounts;

namespace IndustryApp.Application.Matching;

/// <summary>
/// Eşleştirme stratejisinin bulduğu aday usta. Filtre zinciri (onay, uzmanlık, müsaitlik, kapasite)
/// bu alanlar üzerinden çalışır.
/// </summary>
public sealed record MechanicCandidate(
    Guid MechanicId,
    string City,
    double Latitude,
    double Longitude,
    MechanicApprovalStatus ApprovalStatus,
    bool IsUserActive,
    bool IsAvailable,
    short DailyCapacity,
    IReadOnlyCollection<int> SpecialtyCategoryIds,
    double? DistanceKm);
