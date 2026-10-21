namespace IndustryApp.Domain.Reports;

/// <summary>Kaza raporu durumları (T-5.2). Veritabanında metin olarak saklanır.</summary>
public enum ReportStatus
{
    Open,
    OfferSelected,
    Completed,
    Cancelled,
    Expired
}
