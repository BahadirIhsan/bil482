namespace IndustryApp.Domain.Common;

/// <summary>
/// İş kuralı ihlali. Code alanı makinece okunur (ör. REPORT_INVALID_STATE);
/// API bunu ProblemDetails içindeki "code" alanına yazar.
/// </summary>
public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
