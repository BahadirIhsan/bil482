using IndustryApp.Application.Common;

namespace IndustryApp.Application.Reports.Photos;

/// <summary>UC-02 fotoğraf işlemleri: yükleme (14) ve yetkili okuma (15).</summary>
public interface IPhotoService
{
    /// <summary>Boyut ve imzayı denetler, görsel hattından geçirir, temp/{userId}/ altına JPEG olarak yazar.</summary>
    Task<Result<TemporaryPhotoResponse>> UploadTemporaryAsync(IUploadPhotoRequest request, CancellationToken ct);

    /// <summary>Rapor sahibi, rapora eşleşmiş usta veya admin okuyabilir.</summary>
    Task<Result<IPhotoStream>> OpenAsync(Guid photoId, CancellationToken ct);
}
