using System.ComponentModel.DataAnnotations;

namespace IndustryApp.Application.Common.Options;

/// <summary>"Offers" bölümü (T-0.3).</summary>
public sealed class OffersOptions
{
    public const string SectionName = "Offers";

    [Range(1, 24 * 60)]
    public int SelectionUndoWindowMinutes { get; init; }
}
