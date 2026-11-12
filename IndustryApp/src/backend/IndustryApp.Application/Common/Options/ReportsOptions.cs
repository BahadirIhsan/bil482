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

    /// <summary>ReportExpirationJob'un süresi dolan raporları arama aralığı.</summary>
    [Range(5, 3600)]
    public int ExpirationCheckSeconds { get; init; } = 60;

    /// <summary>Bir turda işlenen en fazla rapor sayısı.</summary>
    [Range(1, 1000)]
    public int ExpirationBatchSize { get; init; } = 100;
}
