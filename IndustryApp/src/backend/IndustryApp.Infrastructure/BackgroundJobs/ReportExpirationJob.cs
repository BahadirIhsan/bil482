using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Reports.Expiration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IndustryApp.Infrastructure.BackgroundJobs;

/// <summary>
/// Reports:ExpirationCheckSeconds aralığıyla süresi dolan raporları kapatır (FR-02.4). Her tur yeni bir DI kapsamında
/// çalışır; iş mantığı IReportExpirationService'tedir. Bir turdaki hata loglanır, sonraki tur yine çalışır.
/// </summary>
public sealed class ReportExpirationJob(
    IServiceScopeFactory scopes,
    IOptions<ReportsOptions> options,
    ILogger<ReportExpirationJob> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.ExpirationCheckSeconds));
        do
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IReportExpirationService>();

                // Bir turda batch dolarsa hemen bir sonraki batch'e geçilir.
                while (await service.ExpireDueAsync(stoppingToken) >= options.Value.ExpirationBatchSize)
                {
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Rapor süre dolumu turu başarısız");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }
}
