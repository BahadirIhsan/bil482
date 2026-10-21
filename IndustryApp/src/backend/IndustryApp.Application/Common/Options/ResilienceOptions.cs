using System.ComponentModel.DataAnnotations;

namespace IndustryApp.Application.Common.Options;

/// <summary>"Resilience" bölümü: dış kanalların Retry + Circuit Breaker + Timeout ayarları (PAT-03.6, T-13.3).</summary>
public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    [Required]
    public ResiliencePolicyOptions Email { get; init; } = new();
}

public sealed class ResiliencePolicyOptions
{
    [Range(0, 10)]
    public int RetryCount { get; init; }

    [Range(1, 3600)]
    public int BreakDurationSeconds { get; init; }

    [Range(0.01, 1.0)]
    public double FailureRatio { get; init; }

    [Range(1, 3600)]
    public int SamplingSeconds { get; init; }

    [Range(2, 1000)]
    public int MinimumThroughput { get; init; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; }
}
