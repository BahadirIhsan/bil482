using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Infrastructure.Events;
using IndustryApp.Infrastructure.Persistence;
using IndustryApp.Infrastructure.Persistence.Repositories;
using IndustryApp.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryApp.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    /// <summary>
    /// Kalıcılık, repository'ler, UnitOfWork ve olay altyapısı. Identity, dosya depolama, görsel işleme ve
    /// bildirim kanalları ilgili fazlarda buraya eklenir (T-13.6).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"ConnectionStrings:{ConnectionStringName} tanımlı değil. Yerelde user-secrets, üretimde ortam değişkeni ile verilir.");

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
        services.AddScoped<IMechanicProfileRepository, MechanicProfileRepository>();
        services.AddScoped<IDamageCategoryRepository, DamageCategoryRepository>();
        services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
        services.AddScoped<ILegalDocumentRepository, LegalDocumentRepository>();
        services.AddScoped<IAccidentReportRepository, AccidentReportRepository>();
        services.AddScoped<IReportMatchRepository, ReportMatchRepository>();
        services.AddScoped<IReportStatusHistoryRepository, ReportStatusHistoryRepository>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserConsentRepository, UserConsentRepository>();

        // Singleton'lar DI ile tek örnek; statik Instance alanı yok (PAT-X.2).
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<InProcessEventDispatcher>();
        services.AddSingleton<IEventDispatcher>(sp => sp.GetRequiredService<InProcessEventDispatcher>());
        services.AddHostedService<EventProcessingBackgroundService>();

        return services;
    }
}
