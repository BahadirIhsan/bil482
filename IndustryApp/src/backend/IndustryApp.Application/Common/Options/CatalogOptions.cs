using System.ComponentModel.DataAnnotations;

namespace IndustryApp.Application.Common.Options;

/// <summary>"Catalog" bölümü: hasar ağacı ve araç türlerinin sunucu önbellek süresi (NFR-P1).</summary>
public sealed class CatalogOptions
{
    public const string SectionName = "Catalog";

    [Range(1, 24 * 60)]
    public int CacheMinutes { get; init; } = 10;
}
