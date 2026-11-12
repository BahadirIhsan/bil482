using IndustryApp.Application.Reports.Photos;
using IndustryApp.WebApi.Contracts;
using IndustryApp.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryApp.WebApi.Controllers;

/// <summary>
/// UC-02 fotoğraf uç noktaları (14, 15). Fotoğraflar herkese açık URL ile değil, yetki kontrolüyle bu uçtan
/// blob olarak okunur.
/// </summary>
[Route("api/photos")]
[Authorize]
public sealed class PhotosController(IPhotoService photos) : ApiControllerBase
{
    /// <summary>Multipart gövde sınırı: dosya sınırı servis tarafında Photos:MaxFileSizeMb ile ayrıca uygulanır.</summary>
    private const long MaxRequestBytes = 12L * 1024 * 1024;

    /// <summary>14 · Tek fotoğraf yükler; işlenmiş JPEG geçici anahtarla saklanır.</summary>
    [HttpPost("temporary")]
    [Authorize(Policy = AuthPolicies.CustomerOnly)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType<TemporaryPhotoResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> UploadTemporary(IFormFile file, CancellationToken ct)
    {
        using var request = new FormFilePhotoRequest(file);
        return ToActionResult(await photos.UploadTemporaryAsync(request, ct), value => StatusCode(StatusCodes.Status201Created, value));
    }

    /// <summary>15 · Fotoğrafı döner; rapor sahibi, eşleşmiş usta veya admin okuyabilir.</summary>
    [HttpGet("{photoId:guid}")]
    [Produces("image/jpeg", "application/problem+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid photoId, CancellationToken ct) =>
        ToActionResult(await photos.OpenAsync(photoId, ct), photo =>
        {
            Response.Headers.CacheControl = "private, max-age=3600";
            return File(photo.Content, photo.ContentType);
        });
}
