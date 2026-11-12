namespace IndustryApp.Application.Reports.Photos;

/// <summary>POST /api/photos/temporary ile gelen tek dosya (endpoint 14).</summary>
public interface IUploadPhotoRequest
{
    Stream Content { get; }

    string FileName { get; }

    string ContentType { get; }

    /// <summary>İstemcinin bildirdiği boyut (bayt); asıl kontrol okunan veriyle de yapılır.</summary>
    long Length { get; }
}

/// <summary>İşlenip geçici anahtarla saklanan fotoğraf. TempKey rapor oluştururken geri gönderilir.</summary>
public sealed record TemporaryPhotoResponse(string TempKey, int Width, int Height, long SizeBytes);

/// <summary>GET /api/photos/{photoId} için açılmış fotoğraf (endpoint 15). Akışı çağıran kapatır.</summary>
public interface IPhotoStream
{
    Stream Content { get; }

    string ContentType { get; }
}

public sealed record PhotoStream(Stream Content, string ContentType) : IPhotoStream;

/// <summary>Fotoğrafa erişim kararı için gereken bilgiler; repository tek sorguda döner.</summary>
public sealed record PhotoAccessInfo(
    Guid PhotoId,
    string StorageKey,
    string ContentType,
    Guid ReportId,
    Guid CustomerId,
    IReadOnlyCollection<Guid> MatchedMechanicIds);
