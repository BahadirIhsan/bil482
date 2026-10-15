namespace IndustryApp.Application.Common.Exceptions;

/// <summary>
/// Kayıt sırasında çakışma (PostgreSQL 23505 unique ihlali veya xmin eşzamanlılık hatası).
/// UnitOfWork fırlatır, GlobalExceptionHandler 409'a çevirir.
/// </summary>
public sealed class ConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
