using IndustryApp.Domain.Common;
using IndustryApp.Domain.Events;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports.States;

namespace IndustryApp.Domain.Reports;

/// <summary>
/// Kaza raporu aggregate kökü. Durum Status alanında metin olarak saklanır; her işlem o anki
/// IReportState nesnesine devredilir (PAT-04.1). Raporlar AccidentReportBuilder ile kurulur (PAT-02.1).
/// Teklifler ayrı aggregate köküdür; Offers koleksiyonu seçim/iptal gibi rapor işlemlerinde
/// GetWithOffersAsync ile yüklenir.
/// </summary>
public sealed class AccidentReport : AuditableEntity<Guid>
{
    private readonly List<ReportDamageItem> _damageItems = new();
    private readonly List<ReportPhoto> _photos = new();
    private readonly List<ReportMatch> _matches = new();
    private readonly List<Offer> _offers = new();

    private AccidentReport()
    {
    }

    public Guid CustomerId { get; private set; }

    public short VehicleTypeId { get; private set; }

    public string? Description { get; private set; }

    public string City { get; private set; } = default!;

    public string? District { get; private set; }

    public string? AddressText { get; private set; }

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    public GeoLocation Location => new(Latitude, Longitude);

    public ReportStatus Status { get; private set; }

    public Guid? SelectedOfferId { get; private set; }

    public DateTimeOffset? OfferSelectedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public IReadOnlyCollection<ReportDamageItem> DamageItems => _damageItems;

    public IReadOnlyCollection<ReportPhoto> Photos => _photos;

    public IReadOnlyCollection<ReportMatch> Matches => _matches;

    public IReadOnlyCollection<Offer> Offers => _offers;

    private IReportState State => ReportStateFactory.For(Status);

    /// <summary>Rapor şu an teklif kabul ediyor mu (yalnızca Open).</summary>
    public bool CanReceiveOffers => State.CanReceiveOffers;

    public bool IsOwnedBy(Guid userId) => CustomerId == userId;

    /// <summary>Rapor Open ve usta bu raporla eşleşmiş olmalı (T-5.4).</summary>
    public bool CanReceiveOfferFrom(Guid mechanicId) => CanReceiveOffers && _matches.Any(m => m.MechanicId == mechanicId);

    /// <summary>
    /// İletişim bilgisinin açılabileceği taraf mı: seçim yapılmış (veya tamamlanmış) bir raporun sahibi
    /// müşteri ya da kabul edilmiş teklifin ustası. Diğer herkes için false (FR-04.4).
    /// </summary>
    public bool IsMatchedWith(Guid userId)
    {
        if (Status is not (ReportStatus.OfferSelected or ReportStatus.Completed) || SelectedOfferId is null)
            return false;

        return userId == CustomerId || SelectedOffer?.MechanicId == userId;
    }

    /// <summary>Seçimin geri alınabileceği son an; seçim yoksa null.</summary>
    public DateTimeOffset? UndoDeadline(TimeSpan undoWindow) => OfferSelectedAt + undoWindow;

    private Offer? SelectedOffer => _offers.FirstOrDefault(o => o.Id == SelectedOfferId);

    // ---- Durum işlemleri: karar State nesnesinindir ----

    /// <summary>Teklif bu rapora ait ve Pending olmalı; durum kontrolünü State yapar (yalnızca Open kabul eder).</summary>
    public void SelectOffer(Offer offer, DateTimeOffset now)
    {
        if (offer.ReportId != Id || offer.Status != OfferStatus.Pending)
            throw new DomainException(ErrorCodes.OfferNotAvailable, "Teklif seçilebilir durumda değil.");

        AttachOffer(offer);
        State.SelectOffer(this, offer, now);
    }

    /// <summary>Seçimi geri alır; süre kontrolü OfferSelectedState içindedir.</summary>
    public void UndoSelection(DateTimeOffset now, TimeSpan undoWindow) => State.UndoSelection(this, now, undoWindow);

    public void MarkCompleted(DateTimeOffset now) => State.Complete(this, now);

    public void Cancel(DateTimeOffset now) => State.Cancel(this, now);

    public void Expire(DateTimeOffset now) => State.Expire(this, now);

    public bool IsExpired(DateTimeOffset now) => Status == ReportStatus.Open && ExpiresAt <= now;

    /// <summary>
    /// Eşleştirme sonucunu kaydeder (UC-03). Daha önce eşleşmiş usta atlanır (idempotent).
    /// Hiç eşleşme yoksa da olay yayınlanır; müşteriye "uygun usta bulunamadı" gider.
    /// </summary>
    public IReadOnlyList<ReportMatch> RecordMatches(
        string strategyCode,
        IEnumerable<(Guid MechanicId, decimal? DistanceKm)> mechanics,
        DateTimeOffset now)
    {
        if (!CanReceiveOffers)
            throw new DomainException(ErrorCodes.ReportInvalidState, $"'{Status}' durumundaki rapor için eşleştirme yapılamaz.");

        var added = new List<ReportMatch>();
        foreach (var (mechanicId, distanceKm) in mechanics)
        {
            if (_matches.Any(m => m.MechanicId == mechanicId) || added.Any(m => m.MechanicId == mechanicId))
                continue;
            added.Add(new ReportMatch(Id, mechanicId, strategyCode, distanceKm, now));
        }

        _matches.AddRange(added);
        if (added.Count > 0 || _matches.Count == 0)
            Raise(new MechanicsMatchedEvent(Id, CustomerId, added.Select(m => m.MechanicId).ToList(), now));
        return added;
    }

    internal void AttachOffer(Offer offer)
    {
        if (!_offers.Contains(offer))
            _offers.Add(offer);
    }

    internal bool HasOfferFrom(Guid mechanicId) => _offers.Any(o => o.MechanicId == mechanicId);

    /// <summary>
    /// Durumu değiştirir ve uygun domain olayını üretir. Yalnızca State sınıfları çağırır.
    /// </summary>
    internal void ApplyTransition(ReportStatus target, DateTimeOffset now, Guid? selectedOfferId = null)
    {
        var from = Status;
        switch (target)
        {
            case ReportStatus.OfferSelected:
                var selected = _offers.SingleOrDefault(o => o.Id == selectedOfferId)
                    ?? throw new InvalidOperationException("Seçilen teklif raporun teklifleri arasında yok.");
                var others = _offers
                    .Where(o => o.Id != selected.Id && o.Status == OfferStatus.Pending)
                    .Select(o => o.MechanicId)
                    .Distinct()
                    .ToList();
                SelectedOfferId = selected.Id;
                OfferSelectedAt = now;
                Raise(new OfferSelectedEvent(Id, selected.Id, selected.MechanicId, others, CustomerId, now));
                break;

            case ReportStatus.Open when from == ReportStatus.OfferSelected:
                var mechanics = _offers
                    .Where(o => o.Status is OfferStatus.Accepted or OfferStatus.NotSelected)
                    .Select(o => o.MechanicId)
                    .Distinct()
                    .ToList();
                SelectedOfferId = null;
                OfferSelectedAt = null;
                Raise(new OfferSelectionUndoneEvent(Id, mechanics, now));
                break;

            case ReportStatus.Completed:
                var winner = SelectedOffer
                    ?? throw new InvalidOperationException("Seçilen teklif yüklenmemiş; rapor teklifleriyle birlikte yüklenmeli.");
                CompletedAt = now;
                Raise(new ReportCompletedEvent(Id, winner.MechanicId, now));
                break;

            case ReportStatus.Cancelled:
                var withOffers = _offers
                    .Where(o => o.Status != OfferStatus.Withdrawn)
                    .Select(o => o.MechanicId)
                    .Distinct()
                    .ToList();
                CancelledAt = now;
                Raise(new ReportCancelledEvent(Id, withOffers, now));
                break;

            case ReportStatus.Expired:
                Raise(new ReportExpiredEvent(Id, CustomerId, now));
                break;

            default:
                throw new InvalidOperationException($"'{from}' → '{target}' geçişi tanımlı değil.");
        }

        Status = target;
        Touch(now);
    }

    /// <summary>Yalnızca AccidentReportBuilder çağırır; doğrulamalar Build içinde yapılmıştır.</summary>
    internal static AccidentReport CreateOpen(AccidentReportBuilder b, DateTimeOffset now, TimeSpan ttl)
    {
        var location = b.Location!.Value;
        var report = new AccidentReport
        {
            Id = b.ReportId,
            CustomerId = b.CustomerId,
            VehicleTypeId = b.VehicleTypeId!.Value,
            Description = b.Description,
            City = location.City,
            District = location.District,
            AddressText = location.AddressText,
            Latitude = location.Point.Latitude,
            Longitude = location.Point.Longitude,
            Status = ReportStatus.Open,
            ExpiresAt = now + ttl,
            CreatedAt = now
        };

        report._damageItems.AddRange(b.Damages.Select(d => new ReportDamageItem(report.Id, d.Category.Id, d.Severity, d.Note)));
        report._photos.AddRange(b.Photos.Select(p => new ReportPhoto(p.PhotoId, report.Id, p.StorageKey, p.Meta, p.SortOrder, now)));

        var rootCategoryIds = b.Damages.Select(d => d.Category.EffectiveRootId).Distinct().ToList();
        report.Raise(new AccidentReportCreatedEvent(
            report.Id, report.CustomerId, report.City, report.Latitude, report.Longitude, rootCategoryIds, now));
        return report;
    }
}
