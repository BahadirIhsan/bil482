namespace IndustryApp.Domain.Reports;

/// <summary>Raporun iletildiği usta (UC-03). Anahtar (report_id, mechanic_id) olduğu için eşleştirme idempotenttir.</summary>
public sealed class ReportMatch
{
    private ReportMatch()
    {
    }

    internal ReportMatch(Guid reportId, Guid mechanicId, string strategyCode, decimal? distanceKm, DateTimeOffset now)
    {
        ReportId = reportId;
        MechanicId = mechanicId;
        StrategyCode = strategyCode;
        DistanceKm = distanceKm;
        NotifiedAt = now;
    }

    public Guid ReportId { get; private set; }

    public Guid MechanicId { get; private set; }

    /// <summary>Eşleştirmeyi yapan stratejinin kodu (CITY, RADIUS).</summary>
    public string StrategyCode { get; private set; } = default!;

    public decimal? DistanceKm { get; private set; }

    public DateTimeOffset NotifiedAt { get; private set; }

    public DateTimeOffset? ViewedAt { get; private set; }

    public void MarkViewed(DateTimeOffset now) => ViewedAt ??= now;
}
