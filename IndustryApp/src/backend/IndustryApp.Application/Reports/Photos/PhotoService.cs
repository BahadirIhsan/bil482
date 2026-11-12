using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Common;
using IndustryApp.Application.Common.Options;
using IndustryApp.Domain.Accounts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IndustryApp.Application.Reports.Photos;

/// <summary>
/// Fotoğraf yükleme ve okuma. Görsel tek sefer, yüklemede işlenir (K-04); rapor oluşturma yalnızca hattan geçmiş
/// geçici dosyaları bağlar. Dosyanın nerede durduğunu bilmez (IFileStorage, PAT-02.4).
/// </summary>
public sealed class PhotoService(
    IImageProcessor imageProcessor,
    IFileStorage storage,
    IAccidentReportRepository reports,
    ICurrentUser currentUser,
    IOptions<PhotosOptions> options,
    ILogger<PhotoService> log) : IPhotoService
{
    private const string JpegContentType = "image/jpeg";

    public async Task<Result<TemporaryPhotoResponse>> UploadTemporaryAsync(IUploadPhotoRequest request, CancellationToken ct)
    {
        var maxMb = options.Value.MaxFileSizeMb;
        var maxBytes = maxMb * 1024L * 1024L;
        if (request.Length > maxBytes)
            return Errors.Photos.FileTooLarge(maxMb);

        // En fazla maxBytes + 1 bayt okunur; istemci Length'i yanlış bildirse de sınır aşılamaz.
        await using var buffer = new MemoryStream();
        if (!await CopyWithLimitAsync(request.Content, buffer, maxBytes, ct))
            return Errors.Photos.FileTooLarge(maxMb);

        if (ImageSignature.Detect(buffer.GetBuffer().AsSpan(0, (int)Math.Min(buffer.Length, ImageSignature.HeaderLength))) is null)
            return Errors.Photos.UnsupportedImage;

        buffer.Position = 0;
        IProcessedImage processed;
        try
        {
            processed = await imageProcessor.ProcessAsync(new ImageInput(buffer, request.FileName, request.ContentType), ct);
        }
        catch (UnsupportedImageException ex)
        {
            log.LogInformation(ex, "Fotoğraf çözülemedi: {FileName}", request.FileName);
            return Errors.Photos.UnsupportedImage;
        }

        using (processed)
        {
            await using var jpeg = new MemoryStream();
            await processed.EncodeJpegAsync(jpeg, ct);
            jpeg.Position = 0;

            var key = PhotoKeys.NewTemporary(currentUser.UserId);
            await storage.SaveAsync(jpeg, key, JpegContentType, ct);
            return new TemporaryPhotoResponse(key, processed.Width, processed.Height, jpeg.Length);
        }
    }

    public async Task<Result<IPhotoStream>> OpenAsync(Guid photoId, CancellationToken ct)
    {
        var photo = await reports.GetPhotoAccessAsync(photoId, ct);
        if (photo is null)
            return Errors.Photos.NotFound;

        var userId = currentUser.UserId;
        var allowed = photo.CustomerId == userId
            || currentUser.IsInRole(RoleNames.Admin)
            || photo.MatchedMechanicIds.Contains(userId);
        if (!allowed)
            return currentUser.IsInRole(RoleNames.Mechanic) ? Errors.Offers.NotMatched : Errors.Reports.NotOwner;

        var stream = await storage.OpenReadAsync(photo.StorageKey, ct);
        if (stream is null)
        {
            log.LogWarning("Fotoğraf kaydı var ama dosya yok: {PhotoId} {Key}", photo.PhotoId, photo.StorageKey);
            return Errors.Photos.NotFound;
        }

        return new PhotoStream(stream, photo.ContentType);
    }

    private static async Task<bool> CopyWithLimitAsync(Stream source, Stream destination, long maxBytes, CancellationToken ct)
    {
        var chunk = new byte[81920];
        long total = 0;
        int read;
        while ((read = await source.ReadAsync(chunk, ct)) > 0)
        {
            total += read;
            if (total > maxBytes)
                return false;
            await destination.WriteAsync(chunk.AsMemory(0, read), ct);
        }

        return true;
    }

    private sealed record ImageInput(Stream Content, string FileName, string ContentType) : IImageInput;
}
