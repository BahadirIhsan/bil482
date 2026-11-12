using System.ComponentModel.DataAnnotations;

namespace IndustryApp.Application.Common.Options;

/// <summary>"Photos" bölümü (T-0.3, T-13.5).</summary>
public sealed class PhotosOptions
{
    public const string SectionName = "Photos";

    [Range(1, 50)]
    public int MaxCountPerReport { get; init; }

    [Range(1, 100)]
    public int MaxFileSizeMb { get; init; }

    [Range(320, 10000)]
    public int MaxWidthPx { get; init; }

    [Range(1, 100)]
    public int JpegQuality { get; init; }

    [MinLength(1)]
    public string[] AllowedContentTypes { get; init; } = [];

    /// <summary>Rapora bağlanmayan geçici fotoğraf bu süreden sonra silinir (TemporaryUploadCleanupJob).</summary>
    [Range(1, 24 * 30)]
    public int TemporaryRetentionHours { get; init; } = 24;

    /// <summary>Geçici fotoğraf temizliğinin çalışma aralığı.</summary>
    [Range(1, 24 * 60)]
    public int CleanupIntervalMinutes { get; init; } = 60;
}
