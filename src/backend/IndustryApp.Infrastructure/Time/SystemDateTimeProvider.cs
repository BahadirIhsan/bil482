using IndustryApp.Application.Abstractions;

namespace IndustryApp.Infrastructure.Time;

/// <summary>Gerçek saat. Singleton (PAT-X.2); testlerde sabit saatli sahte sürümle değiştirilir.</summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
