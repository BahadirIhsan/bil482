namespace IndustryApp.Application.Abstractions;

/// <summary>Saat kaynağı. Singleton; testlerde sabit saatli sahte sürümle değiştirilir (PAT-X.2).</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
