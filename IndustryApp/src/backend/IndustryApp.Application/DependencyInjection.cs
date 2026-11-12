using FluentValidation;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Accounts;
using IndustryApp.Application.Accounts.Factories;
using IndustryApp.Application.Catalog;
using IndustryApp.Application.Contacts;
using IndustryApp.Application.Notifications;
using IndustryApp.Application.Offers;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Offers.Sorting;
using IndustryApp.Application.Reports;
using IndustryApp.Application.Reports.Expiration;
using IndustryApp.Application.Reports.Photos;
using IndustryApp.Application.Selection;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryApp.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Application servisleri, validator'lar ve olay handler'ları. Servisler ve desen sınıfları
    /// (fabrikalar, stratejiler, filtreler) ilgili fazlarda buraya eklenir (T-13.6).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IUnitOfWork>(includeInternalTypes: true);

        // UC-02: rapor oluşturma (Builder), fotoğraf yükleme ve süre dolumu; katalog önbelleği.
        services.AddMemoryCache();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IReportExpirationService, ReportExpirationService>();

        // UC-04: seçim (Command + State), iletişim (Proxy), rapor detayı / tamamlama / iptal.
        services.AddScoped<IReportCommandFactory, ReportCommandFactory>();
        services.AddScoped<IOfferSelectionService, OfferSelectionService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IReportService, ReportService>();

        // UC-04: teklif listesi ve sıralama stratejileri (PAT-04.4). Yeni sıralama = yeni sınıf + bir kayıt satırı.
        services.AddScoped<IOfferSortStrategy, PriceAscendingSort>();
        services.AddScoped<IOfferSortStrategy, FastestCompletionSort>();
        services.AddScoped<IOfferSortStrategy, NewestFirstSort>();
        services.AddScoped<IOfferService, OfferService>();

        // UC-01: kayıt fabrikaları (PAT-01.1), kimlik, hesap ve kayıt formu katalogları.
        services.AddScoped<IUserProfileFactory, CustomerProfileFactory>();
        services.AddScoped<IUserProfileFactory, MechanicProfileFactory>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICatalogService, CatalogService>();

        // Bildirim hattı (T-9.3); handler'lar aşağıdaki taramayla gelir.
        services.AddScoped<INotificationService, NotificationService>();

        // Tüm IEventHandler<T> implementasyonları; yeni handler kayıt unutulmadan devreye girer.
        services.Scan(scan => scan
            .FromAssemblyOf<IUnitOfWork>()
            .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
