using FluentValidation;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Contacts;
using IndustryApp.Application.Offers.Commands;
using IndustryApp.Application.Reports;
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

        // UC-04: seçim (Command + State), iletişim (Proxy), rapor detayı / tamamlama / iptal.
        services.AddScoped<IReportCommandFactory, ReportCommandFactory>();
        services.AddScoped<IOfferSelectionService, OfferSelectionService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IReportService, ReportService>();

        // Tüm IEventHandler<T> implementasyonları; yeni handler kayıt unutulmadan devreye girer.
        services.Scan(scan => scan
            .FromAssemblyOf<IUnitOfWork>()
            .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
