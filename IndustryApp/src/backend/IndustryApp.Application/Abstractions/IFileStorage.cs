namespace IndustryApp.Application.Abstractions;

/// <summary>Dosya depolama portu (PAT-02.4). Bugün sunucu diski, ileride S3/MinIO; Application değişmez.</summary>
public interface IFileStorage
{
    Task SaveAsync(Stream content, string key, string contentType, CancellationToken ct);

    Task<Stream?> OpenReadAsync(string key, CancellationToken ct);

    Task MoveAsync(string sourceKey, string targetKey, CancellationToken ct);

    Task DeleteAsync(string key, CancellationToken ct);
}
