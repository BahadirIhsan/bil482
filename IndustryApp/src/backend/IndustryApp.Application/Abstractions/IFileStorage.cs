namespace IndustryApp.Application.Abstractions;

/// <summary>
/// Dosya depolama portu (PAT-02.4). Bugün sunucu diski (LocalFileStorage), ileride S3/MinIO; Application değişmez.
/// Anahtarlar "/" ile ayrılmış göreli yollardır (ör. temp/{userId}/{guid}.jpg); geçersiz anahtar ArgumentException fırlatır.
/// </summary>
public interface IFileStorage
{
    Task SaveAsync(Stream content, string key, string contentType, CancellationToken ct);

    /// <summary>Dosyayı okumak için açar; yoksa null.</summary>
    Task<Stream?> OpenReadAsync(string key, CancellationToken ct);

    /// <summary>Dosyanın bayt cinsinden boyutu; yoksa null.</summary>
    Task<long?> GetSizeAsync(string key, CancellationToken ct);

    /// <summary>Kaynak yoksa FileNotFoundException fırlatır; hedef klasör yoksa oluşturulur.</summary>
    Task MoveAsync(string sourceKey, string targetKey, CancellationToken ct);

    /// <summary>Dosya yoksa sessizce döner.</summary>
    Task DeleteAsync(string key, CancellationToken ct);

    /// <summary>Önekin altındaki, son yazılma zamanı verilen andan eski dosyaların anahtarları.</summary>
    Task<IReadOnlyList<string>> ListOlderThanAsync(string prefix, DateTimeOffset olderThan, CancellationToken ct);
}
