using IndustryApp.Domain.Reports;

namespace IndustryApp.Application.Reports.Contracts;

/// <summary>
/// POST /api/reports gövdesi (endpoint 16). Fotoğraflar önceden endpoint 14 ile yüklenmiş geçici anahtarlardır;
/// sıraları rapordaki gösterim sırasıdır. Koordinat cihazdan alınmadıysa istemci ilin merkezini gönderir ve ilçe zorunludur.
/// </summary>
public interface ICreateReportRequest
{
    short VehicleTypeId { get; }

    string? Description { get; }

    string? City { get; }

    string? District { get; }

    string? AddressText { get; }

    double? Latitude { get; }

    double? Longitude { get; }

    /// <summary>Koordinat cihazın konumundan mı geldi (izin verildi mi). false ise ilçe zorunludur (UC-02 5a).</summary>
    bool IsDeviceLocation { get; }

    IReadOnlyList<ReportDamageInput>? Damages { get; }

    IReadOnlyList<string>? PhotoKeys { get; }
}

/// <summary>Sihirbazın 2. adımında seçilen hasar bölgesi ve şiddeti.</summary>
public sealed record ReportDamageInput(int CategoryId, DamageSeverity Severity, string? Note = null);

/// <summary>Oluşturulan raporun özeti; istemci rapor detayına yönlenir.</summary>
public sealed record CreateReportResponse(Guid Id, ReportStatus Status, DateTimeOffset ExpiresAt);
