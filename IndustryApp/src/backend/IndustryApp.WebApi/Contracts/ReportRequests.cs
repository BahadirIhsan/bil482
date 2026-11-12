using IndustryApp.Application.Reports.Contracts;
using IndustryApp.Application.Reports.Photos;

namespace IndustryApp.WebApi.Contracts;

/// <summary>POST /api/reports gövdesi (endpoint 16). Müşteri kimliği istemciden alınmaz, token'dan okunur.</summary>
public sealed record CreateReportRequest(
    short VehicleTypeId,
    string? Description,
    string? City,
    string? District,
    string? AddressText,
    double? Latitude,
    double? Longitude,
    bool IsDeviceLocation,
    IReadOnlyList<ReportDamageInput>? Damages,
    IReadOnlyList<string>? PhotoKeys) : ICreateReportRequest;

/// <summary>Multipart dosyayı Application'ın IUploadPhotoRequest arayüzüne uyarlar (endpoint 14).</summary>
public sealed class FormFilePhotoRequest(IFormFile file) : IUploadPhotoRequest, IDisposable
{
    private readonly Stream _content = file.OpenReadStream();

    public Stream Content => _content;

    public string FileName => file.FileName;

    public string ContentType => file.ContentType;

    public long Length => file.Length;

    public void Dispose() => _content.Dispose();
}
