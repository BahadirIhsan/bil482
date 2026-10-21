using System.ComponentModel.DataAnnotations;

namespace IndustryApp.Application.Common.Options;

/// <summary>"Matching" bölümü. Filters listesi Chain of Responsibility sırasını belirler (T-13.5).</summary>
public sealed class MatchingOptions
{
    public const string SectionName = "Matching";

    /// <summary>Aktif eşleştirme stratejisinin kodu: CITY veya RADIUS.</summary>
    [Required]
    public string Strategy { get; init; } = default!;

    [Range(0.1, 500)]
    public double DefaultRadiusKm { get; init; }

    [Range(1, 1000)]
    public int MaxMechanicsPerReport { get; init; }

    [MinLength(1)]
    public string[] Filters { get; init; } = [];
}
