using System.Globalization;
using System.Text.RegularExpressions;

namespace IndustryApp.Application.Reports.Photos;

/// <summary>
/// Fotoğraf depolama anahtarları (SDD 7.2): yüklemede temp/{userId}/{guid}.jpg, rapor oluşturulunca
/// reports/{yyyy}/{MM}/{reportId}/{photoId}.jpg.
/// </summary>
public static partial class PhotoKeys
{
    public const string TemporaryRoot = "temp";

    public static string NewTemporary(Guid userId) => $"{TemporaryRoot}/{userId}/{Guid.NewGuid()}.jpg";

    /// <summary>Anahtar bu kullanıcının geçici klasöründe ve beklenen biçimde mi (UC-02 7b).</summary>
    public static bool IsTemporaryOf(string? key, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var match = TemporaryPattern().Match(key);
        return match.Success && Guid.TryParse(match.Groups["user"].Value, out var owner) && owner == userId;
    }

    public static string Final(DateTimeOffset createdAt, Guid reportId, Guid photoId) =>
        string.Create(CultureInfo.InvariantCulture, $"reports/{createdAt:yyyy}/{createdAt:MM}/{reportId}/{photoId}.jpg");

    [GeneratedRegex(@"^temp/(?<user>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\.jpg$")]
    private static partial Regex TemporaryPattern();
}
