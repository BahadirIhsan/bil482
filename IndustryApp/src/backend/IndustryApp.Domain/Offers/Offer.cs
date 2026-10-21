using IndustryApp.Domain.Common;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Offers.Pricing;
using IndustryApp.Domain.Reports;

namespace IndustryApp.Domain.Offers;

/// <summary>
/// Ustanın kapalı teklifi (K-06). Ayrı aggregate köküdür: ustalar aynı rapora aynı anda teklif verir,
/// teklifler raporun içinde olsaydı her teklif tüm raporu kilitlerdi (T-5.1).
/// </summary>
public sealed class Offer : AuditableEntity<Guid>
{
    /// <summary>offers.currency kolonunun varsayılanı (T-6.6).</summary>
    public const string DefaultCurrency = "TRY";

    private readonly List<OfferPriceItem> _priceItems = new();

    private Offer()
    {
    }

    public Guid ReportId { get; private set; }

    public Guid MechanicId { get; private set; }

    public decimal TotalPrice { get; private set; }

    public string Currency { get; private set; } = DefaultCurrency;

    public short EstimatedDays { get; private set; }

    public string? Note { get; private set; }

    public OfferStatus Status { get; private set; }

    public DateTimeOffset? DecidedAt { get; private set; }

    public IReadOnlyCollection<OfferPriceItem> PriceItems => _priceItems;

    /// <summary>
    /// Teklif oluşturur. Toplam fiyat Decorator zincirinden hesaplanır ve 0'dan büyük olmalıdır;
    /// rapor teklif kabul eden durumda (Open) olmalıdır. Eşleşme kontrolü serviste yapılır.
    /// </summary>
    public static Offer Create(
        AccidentReport report,
        Guid mechanicId,
        IPriceComponent price,
        short estimatedDays,
        string? note,
        DateTimeOffset now)
    {
        if (!report.CanReceiveOffers)
            throw new DomainException(ErrorCodes.ReportInvalidState, "Rapor artık teklif kabul etmiyor.");
        if (report.HasOfferFrom(mechanicId))
            throw new DomainException(ErrorCodes.OfferAlreadyExists, "Bu rapora zaten teklif verdiniz.");

        var offer = new Offer
        {
            Id = Guid.NewGuid(),
            ReportId = report.Id,
            MechanicId = mechanicId,
            Currency = DefaultCurrency,
            Status = OfferStatus.Pending,
            CreatedAt = now
        };
        offer.SetPricing(price, estimatedDays, note);
        report.AttachOffer(offer);
        offer.Raise(new OfferSubmittedEvent(offer.Id, report.Id, mechanicId, report.CustomerId, offer.TotalPrice, now));
        return offer;
    }

    /// <summary>Yalnızca Pending teklif ve Open rapor için; kalemler silinip yeniden yazılır.</summary>
    public void UpdatePricing(AccidentReport report, IPriceComponent price, short estimatedDays, string? note, DateTimeOffset now)
    {
        EnsureBelongsTo(report);
        EnsureStatus(OfferStatus.Pending, nameof(UpdatePricing));
        if (!report.CanReceiveOffers)
            throw new DomainException(ErrorCodes.ReportInvalidState, "Rapor artık teklif kabul etmiyor.");

        SetPricing(price, estimatedDays, note);
        Touch(now);
        Raise(new OfferUpdatedEvent(Id, ReportId, report.CustomerId, TotalPrice, now));
    }

    /// <summary>Usta yalnızca Pending teklifini geri çekebilir.</summary>
    public void Withdraw(AccidentReport report, DateTimeOffset now)
    {
        EnsureBelongsTo(report);
        EnsureStatus(OfferStatus.Pending, nameof(Withdraw));

        Status = OfferStatus.Withdrawn;
        DecidedAt = now;
        Touch(now);
        Raise(new OfferWithdrawnEvent(Id, ReportId, report.CustomerId, now));
    }

    /// <summary>Seçim komutu: bu teklif kabul edildi.</summary>
    public void Accept(DateTimeOffset now)
    {
        EnsureStatus(OfferStatus.Pending, nameof(Accept));
        Status = OfferStatus.Accepted;
        DecidedAt = now;
        Touch(now);
    }

    /// <summary>Seçim komutu: başka bir teklif seçildi.</summary>
    public void MarkNotSelected(DateTimeOffset now)
    {
        EnsureStatus(OfferStatus.Pending, nameof(MarkNotSelected));
        Status = OfferStatus.NotSelected;
        DecidedAt = now;
        Touch(now);
    }

    /// <summary>Seçimin geri alınması: Accepted veya NotSelected teklif tekrar Pending olur.</summary>
    public void Reopen(DateTimeOffset now)
    {
        if (Status is not (OfferStatus.Accepted or OfferStatus.NotSelected))
            throw InvalidState(nameof(Reopen));

        Status = OfferStatus.Pending;
        DecidedAt = null;
        Touch(now);
    }

    /// <summary>Rapor iptal edildiğinde veya süresi dolduğunda teklif kapanır.</summary>
    public void Close(DateTimeOffset now)
    {
        if (Status is OfferStatus.Withdrawn or OfferStatus.Closed)
            throw InvalidState(nameof(Close));

        Status = OfferStatus.Closed;
        DecidedAt = now;
        Touch(now);
    }

    private void SetPricing(IPriceComponent price, short estimatedDays, string? note)
    {
        var lines = price.Lines;
        if (!lines.Any(l => l.Type == PriceItemType.Labor))
            throw new DomainException(ErrorCodes.OfferLaborRequired, "Teklifte işçilik kalemi zorunludur.");
        if (price.Total <= 0)
            throw new DomainException(ErrorCodes.OfferTotalInvalid, "Teklif toplamı sıfırdan büyük olmalıdır.");
        if (estimatedDays <= 0)
            throw new DomainException(ErrorCodes.OfferEstimatedDaysInvalid, "Tahmini süre en az 1 gün olmalıdır.");

        TotalPrice = price.Total;
        EstimatedDays = estimatedDays;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        _priceItems.Clear();
        _priceItems.AddRange(lines.Select((line, index) => new OfferPriceItem(Id, line, (short)index)));
    }

    private void EnsureBelongsTo(AccidentReport report)
    {
        if (report.Id != ReportId)
            throw new InvalidOperationException("Teklif bu rapora ait değil.");
    }

    private void EnsureStatus(OfferStatus expected, string operation)
    {
        if (Status != expected)
            throw InvalidState(operation);
    }

    private DomainException InvalidState(string operation) =>
        new(ErrorCodes.OfferInvalidState, $"'{Status}' durumundaki teklif için '{operation}' yapılamaz.");
}
