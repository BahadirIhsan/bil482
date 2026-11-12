namespace IndustryApp.Application.Reports.Expiration;

/// <summary>Süresi dolan Open raporları kapatır (FR-02.4). ReportExpirationJob periyodik olarak çağırır.</summary>
public interface IReportExpirationService
{
    /// <summary>Bir tur çalışır; Expired yapılan rapor sayısını döner.</summary>
    Task<int> ExpireDueAsync(CancellationToken ct);
}
