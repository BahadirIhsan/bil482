using IndustryApp.Application.Common.Options;

namespace IndustryApp.WebApi.Extensions;

public static class OptionsExtensions
{
    /// <summary>
    /// Süre, sınır ve adresler yalnızca appsettings'ten okunur, koda gömülmez (T-0.3, NFR-M2).
    /// Geçersiz değer uygulama açılırken hata verir; yanlış ayar sessizce geçmez.
    /// </summary>
    public static IServiceCollection AddAppOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidated<ReportsOptions>(configuration, ReportsOptions.SectionName);
        services.AddValidated<OffersOptions>(configuration, OffersOptions.SectionName);
        services.AddValidated<MatchingOptions>(configuration, MatchingOptions.SectionName);
        services.AddValidated<PhotosOptions>(configuration, PhotosOptions.SectionName);
        services.AddValidated<StorageOptions>(configuration, StorageOptions.SectionName);
        services.AddValidated<EmailOptions>(configuration, EmailOptions.SectionName);
        services.AddValidated<ResilienceOptions>(configuration, ResilienceOptions.SectionName);
        services.AddValidated<JwtOptions>(configuration, JwtOptions.SectionName);
        return services;
    }

    private static void AddValidated<TOptions>(this IServiceCollection services, IConfiguration configuration, string section)
        where TOptions : class =>
        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(section))
            .ValidateDataAnnotations()
            .ValidateOnStart();
}
