using System.ComponentModel.DataAnnotations;

namespace IndustryApp.Application.Common.Options;

/// <summary>"Storage" bölümü: LocalFileStorage kök klasörü (yerelde ./.uploads, üretimde /data/uploads).</summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    [Required]
    public string RootPath { get; init; } = default!;
}
