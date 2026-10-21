using System.ComponentModel.DataAnnotations;

namespace IndustryApp.Application.Common.Options;

/// <summary>"Reports" bölümü (T-0.3, T-13.5).</summary>
public sealed class ReportsOptions
{
    public const string SectionName = "Reports";

    /// <summary>Teklif seçilmeyen rapor bu süre sonunda Expired olur. Test için kesirli değer verilebilir (0.1 = 6 dk).</summary>
    [Range(0.01, 24 * 365)]
    public double ExpireAfterHours { get; init; }

    [Range(1, 100)]
    public int MaxOpenPerCustomer { get; init; }
}
