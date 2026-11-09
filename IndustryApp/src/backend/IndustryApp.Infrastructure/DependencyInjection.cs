using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Infrastructure.Events;
using IndustryApp.Infrastructure.Identity;
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
    /// Kalıcılık, repository'ler, UnitOfWork, olay altyapısı ve kimlik (Identity, JWT, CurrentUser).
    /// Dosya depolama, görsel işleme ve bildirim kanalları ilgili fazlarda buraya eklenir (T-13.6).
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
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Kimlik (UC-01, K-11): Identity çekirdeği yalnızca IdentityService içinden kullanılır.
        services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireDigit = true;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireLowercase = false;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                o.Lockout.AllowedForNewUsers = true;
                o.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        // Singleton'lar DI ile tek örnek; statik Instance alanı yok (PAT-X.2).
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<InProcessEventDispatcher>();
        services.AddSingleton<IEventDispatcher>(sp => sp.GetRequiredService<InProcessEventDispatcher>());
        services.AddHostedService<EventProcessingBackgroundService>();

        return services;
    }
}
