using System.ComponentModel.DataAnnotations;

namespace IndustryApp.Application.Common.Options;

/// <summary>"Email" bölümü. Host, User ve Password üretimde ortam değişkeninden gelir (Email__Host ...).</summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; init; } = default!;

    [Range(1, 65535)]
    public int Port { get; init; }

    public bool UseStartTls { get; init; }

    public string? User { get; init; }

    public string? Password { get; init; }

    [Required]
    public string FromAddress { get; init; } = default!;

    [Required]
    public string FromName { get; init; } = default!;
}
