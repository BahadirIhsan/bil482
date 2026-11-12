using System.Text.RegularExpressions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace IndustryApp.Infrastructure.Storage;

// [UC-02][PAT-02.4] Adapter
// Niyet   : .NET dosya sistemi API'sini Application'ın beklediği IFileStorage arayüzüne uyarlamak.
// Neden   : Servisler System.IO'yu doğrudan kullansaydı dosyanın yeri (disk, S3, MinIO) her serviste bilinir,
//           yol birleştirme ve güvenlik kontrolleri dağılırdı.
// Kazanım : Depolama değişince yalnızca yeni bir adaptör yazılır; path traversal koruması tek yerdedir.
// Metrik  : Depolama değişikliğinde Application'da değişen satır: 0 (LocalFileStorage_PathTraversal_Throws, TC-02.1.2).
/// <summary>
/// Dosyaları Storage:RootPath altında saklar (yerelde ./.uploads, üretimde /data/uploads birimi). Anahtar yalnızca
/// harf, rakam, '-', '_', '.' ve '/' içerebilir; '..' parçası ve kökün dışına çıkan yol ArgumentException fırlatır.
/// </summary>
public sealed partial class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IOptions<StorageOptions> options)
    {
        _root = Path.GetFullPath(options.Value.RootPath);
        Directory.CreateDirectory(_root);
    }

    public async Task SaveAsync(Stream content, string key, string contentType, CancellationToken ct)
    {
        var path = Resolve(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        // Önce geçici dosyaya yazılır, sonra yeniden adlandırılır: yarım dosya hiçbir zaman anahtarla görünmez.
        var partial = path + ".partial";
        await using (var file = new FileStream(partial, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            await content.CopyToAsync(file, ct);
        File.Move(partial, path, overwrite: true);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken ct)
    {
        var path = Resolve(key);
        Stream? stream = File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true)
            : null;
        return Task.FromResult(stream);
    }

    public Task<long?> GetSizeAsync(string key, CancellationToken ct)
    {
        var info = new FileInfo(Resolve(key));
        return Task.FromResult(info.Exists ? info.Length : (long?)null);
    }

    public Task MoveAsync(string sourceKey, string targetKey, CancellationToken ct)
    {
        var source = Resolve(sourceKey);
        var target = Resolve(targetKey);
        if (!File.Exists(source))
            throw new FileNotFoundException("Taşınacak dosya bulunamadı.", sourceKey);

        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Move(source, target, overwrite: false);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        var path = Resolve(key);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> ListOlderThanAsync(string prefix, DateTimeOffset olderThan, CancellationToken ct)
    {
        var directory = Resolve(prefix);
        if (!Directory.Exists(directory))
            return Task.FromResult<IReadOnlyList<string>>([]);

        IReadOnlyList<string> keys = Directory
            .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".partial", StringComparison.Ordinal))
            .Where(path => File.GetLastWriteTimeUtc(path) < olderThan.UtcDateTime)
            .Select(path => Path.GetRelativePath(_root, path).Replace(Path.DirectorySeparatorChar, '/'))
            .ToList();
        return Task.FromResult(keys);
    }

    /// <summary>Anahtarı kökün altındaki mutlak yola çevirir; kökün dışına çıkan veya biçimsiz anahtar reddedilir.</summary>
    private string Resolve(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !KeyPattern().IsMatch(key) || key.Split('/').Any(part => part is "" or "." or ".."))
            throw new ArgumentException($"Geçersiz depolama anahtarı: '{key}'.", nameof(key));

        var full = Path.GetFullPath(Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException($"Depolama anahtarı kök klasörün dışını gösteriyor: '{key}'.", nameof(key));
        return full;
    }

    [GeneratedRegex(@"^[A-Za-z0-9_\-./]+$")]
    private static partial Regex KeyPattern();
}
