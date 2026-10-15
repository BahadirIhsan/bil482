using IndustryApp.Domain.Catalog;
using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Reports;

// [UC-02][PAT-02.1] Builder
// Niyet   : Beş sihirbaz adımında toplanan veriden geçerli bir kaza raporu kurmak.
// Neden   : Rapor ancak en az 1 fotoğraf, 1 hasar kalemi, konum ve araç türüyle geçerlidir; çok parametreli
//           bir constructor ya da yarım doldurulmuş entity veritabanına ulaşabilirdi.
// Kazanım : Geçerlilik kuralı tek yerde ve test edilebilir; yeni isteğe bağlı alan yeni bir builder metodudur,
//           çağıranlar değişmez.
// Metrik  : Eksik raporların %100'ü reddedilir (Builder_MissingPhoto_Throws, TC-02.3.1).
public sealed class AccidentReportBuilder
{
    private readonly List<(DamageCategory Category, DamageSeverity Severity, string? Note)> _damages = new();
    private readonly List<(Guid PhotoId, string StorageKey, PhotoMetadata Meta, short SortOrder)> _photos = new();

    /// <summary>Fotoğraf anahtarları (reports/{yyyy}/{MM}/{reportId}/...) rapor kaydedilmeden önce gerektiği için kimlik baştan üretilir.</summary>
    public Guid ReportId { get; } = Guid.NewGuid();

    internal Guid CustomerId { get; private set; }

    internal short? VehicleTypeId { get; private set; }

    internal string? Description { get; private set; }

    internal (string City, string? District, string? AddressText, GeoLocation Point)? Location { get; private set; }

    internal IReadOnlyList<(DamageCategory Category, DamageSeverity Severity, string? Note)> Damages => _damages;

    internal IReadOnlyList<(Guid PhotoId, string StorageKey, PhotoMetadata Meta, short SortOrder)> Photos => _photos;

    public AccidentReportBuilder WithCustomer(Guid customerId)
    {
        CustomerId = customerId;
        return this;
    }

    public AccidentReportBuilder WithVehicleType(short vehicleTypeId)
    {
        VehicleTypeId = vehicleTypeId;
        return this;
    }

    public AccidentReportBuilder WithDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        return this;
    }

    public AccidentReportBuilder AtLocation(string city, string? district, double latitude, double longitude, string? addressText = null)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException(ErrorCodes.InvalidLocation, "Konum için il zorunludur.");

        Location = (
            city.Trim(),
            string.IsNullOrWhiteSpace(district) ? null : district.Trim(),
            string.IsNullOrWhiteSpace(addressText) ? null : addressText.Trim(),
            new GeoLocation(latitude, longitude));
        return this;
    }

    /// <summary>
    /// Hasar kalemi ekler. Kategori nesnesi alınır; böylece pasif kategori reddedilir (UC-02 7b) ve
    /// eşleştirme için kök kategori (root_id) olaya yazılabilir.
    /// </summary>
    public AccidentReportBuilder AddDamage(DamageCategory category, DamageSeverity severity, string? note = null)
    {
        if (!category.IsActive)
            throw new DomainException(ErrorCodes.CategoryInactive, $"'{category.Name}' kategorisi artık kullanılmıyor.");
        if (_damages.Any(d => d.Category.Id == category.Id))
            throw new DomainException(ErrorCodes.DuplicateDamageItem, $"'{category.Name}' hasar kalemi zaten eklendi.");

        _damages.Add((category, severity, string.IsNullOrWhiteSpace(note) ? null : note.Trim()));
        return this;
    }

    public AccidentReportBuilder AddPhoto(Guid photoId, string storageKey, PhotoMetadata meta, int? sortOrder = null)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException("Fotoğraf anahtarı boş olamaz.", nameof(storageKey));

        _photos.Add((photoId, storageKey, meta, (short)(sortOrder ?? _photos.Count)));
        return this;
    }

    /// <summary>Eksik bilgi varsa REPORT_INCOMPLETE fırlatır; geçerliyse AccidentReportCreatedEvent üreten Open bir rapor döner.</summary>
    public AccidentReport Build(DateTimeOffset now, TimeSpan ttl)
    {
        var missing = new List<string>();
        if (CustomerId == Guid.Empty) missing.Add("müşteri");
        if (VehicleTypeId is null) missing.Add("araç türü");
        if (Location is null) missing.Add("konum");
        if (_damages.Count == 0) missing.Add("hasar bölgesi");
        if (_photos.Count == 0) missing.Add("fotoğraf");

        if (missing.Count > 0)
            throw new DomainException(ErrorCodes.ReportIncomplete, $"Rapor eksik bilgi içeriyor: {string.Join(", ", missing)}.");
        if (ttl <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttl), "Rapor süresi pozitif olmalıdır.");

        return AccidentReport.CreateOpen(this, now, ttl);
    }
}
