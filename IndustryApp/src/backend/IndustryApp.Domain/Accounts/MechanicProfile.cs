using IndustryApp.Domain.Common;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Reports;

namespace IndustryApp.Domain.Accounts;

/// <summary>
/// Usta (atölye) profili. Yeni hesap PendingApproval durumunda açılır ve admin onayına kadar
/// eşleştirmeye girmez (K-08, FR-01.3).
/// </summary>
public sealed class MechanicProfile : AuditableEntity<Guid>
{
    /// <summary>mechanic_profiles.daily_capacity kolonunun varsayılanı (T-6.3).</summary>
    public const short DefaultDailyCapacity = 5;

    private readonly List<MechanicSpecialty> _specialties = new();

    private MechanicProfile()
    {
    }

    public Guid UserId => Id;

    public string WorkshopName { get; private set; } = default!;

    public string TaxNumber { get; private set; } = default!;

    public string City { get; private set; } = default!;

    public string District { get; private set; } = default!;

    public string Address { get; private set; } = default!;

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    public GeoLocation Location => new(Latitude, Longitude);

    public MechanicApprovalStatus ApprovalStatus { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTimeOffset? ApprovedAt { get; private set; }

    public Guid? ApprovedBy { get; private set; }

    public bool IsAvailable { get; private set; }

    public short DailyCapacity { get; private set; }

    public IReadOnlyCollection<MechanicSpecialty> Specialties => _specialties;

    public bool IsApproved => ApprovalStatus == MechanicApprovalStatus.Approved;

    public static MechanicProfile Create(
        Guid userId,
        string workshopName,
        string taxNumber,
        string city,
        string district,
        string address,
        GeoLocation location,
        IEnumerable<int> specialtyCategoryIds,
        DateTimeOffset now)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));

        var specialties = specialtyCategoryIds.Distinct().ToList();
        if (specialties.Count == 0)
            throw new DomainException(ErrorCodes.MechanicSpecialtyRequired, "En az bir uzmanlık seçilmelidir.");

        var profile = new MechanicProfile
        {
            Id = userId,
            WorkshopName = workshopName.Trim(),
            TaxNumber = taxNumber.Trim(),
            City = city.Trim(),
            District = district.Trim(),
            Address = address.Trim(),
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            ApprovalStatus = MechanicApprovalStatus.PendingApproval,
            IsAvailable = true,
            DailyCapacity = DefaultDailyCapacity,
            CreatedAt = now
        };
        profile._specialties.AddRange(specialties.Select(id => new MechanicSpecialty(userId, id)));
        return profile;
    }

    public bool HasSpecialtyIn(IEnumerable<int> rootCategoryIds) =>
        _specialties.Any(s => rootCategoryIds.Contains(s.DamageCategoryId));

    /// <summary>Admin onayı (YA-01). Bekleyen veya reddedilmiş usta onaylanabilir.</summary>
    public void Approve(Guid adminId, DateTimeOffset now)
    {
        if (ApprovalStatus is not (MechanicApprovalStatus.PendingApproval or MechanicApprovalStatus.Rejected))
            throw new DomainException(ErrorCodes.MechanicInvalidStatus, $"'{ApprovalStatus}' durumundaki usta onaylanamaz.");

        ApprovalStatus = MechanicApprovalStatus.Approved;
        ApprovedAt = now;
        ApprovedBy = adminId;
        RejectionReason = null;
        Touch(now);
        Raise(new MechanicApprovedEvent(Id, now));
    }

    /// <summary>Admin reddi (YA-01). Gerekçe zorunludur; usta bunu durum ekranında görür.</summary>
    public void Reject(string reason, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(ErrorCodes.RejectionReasonRequired, "Ret gerekçesi zorunludur.");
        if (ApprovalStatus != MechanicApprovalStatus.PendingApproval)
            throw new DomainException(ErrorCodes.MechanicInvalidStatus, $"'{ApprovalStatus}' durumundaki usta reddedilemez.");

        ApprovalStatus = MechanicApprovalStatus.Rejected;
        RejectionReason = reason.Trim();
        Touch(now);
        Raise(new MechanicRejectedEvent(Id, RejectionReason, now));
    }

    public void SetAvailability(bool isAvailable, DateTimeOffset now)
    {
        IsAvailable = isAvailable;
        Touch(now);
    }
}
