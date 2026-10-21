namespace IndustryApp.Domain.Reports.States;

/// <summary>
/// Status değerine karşılık gelen durum nesnesini döner. Durum sınıfları alan taşımadığı için
/// her duruma tek bir örnek paylaşılır.
/// </summary>
public static class ReportStateFactory
{
    private static readonly IReportState Open = new OpenState();
    private static readonly IReportState OfferSelected = new OfferSelectedState();
    private static readonly IReportState Completed = new CompletedState();
    private static readonly IReportState Cancelled = new CancelledState();
    private static readonly IReportState Expired = new ExpiredState();

    public static IReportState For(ReportStatus status) => status switch
    {
        ReportStatus.Open => Open,
        ReportStatus.OfferSelected => OfferSelected,
        ReportStatus.Completed => Completed,
        ReportStatus.Cancelled => Cancelled,
        ReportStatus.Expired => Expired,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Bilinmeyen rapor durumu.")
    };
}
