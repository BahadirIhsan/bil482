using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Common.Options;
using IndustryApp.Application.Reports.Photos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IndustryApp.Infrastructure.BackgroundJobs;

/// <summary>
/// temp/ altında Photos:TemporaryRetentionHours'tan eski dosyaları siler. Rapora bağlanan fotoğraf oluşturma anında
/// reports/ altına taşındığı için temp/ altında kalan her dosya kullanılmayan bir yüklemedir.
/// </summary>
public sealed class TemporaryUploadCleanupJob(
    IFileStorage storage,
    IDateTimeProvider clock,
    IOptions<PhotosOptions> options,
    ILogger<TemporaryUploadCleanupJob> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.Value.CleanupIntervalMinutes));
        do
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Geçici fotoğraf temizliği başarısız");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }

    /// <summary>Tek tur; silinen dosya sayısını döner.</summary>
    public async Task<int> RunOnceAsync(CancellationToken ct)
    {
        var cutoff = clock.UtcNow.AddHours(-options.Value.TemporaryRetentionHours);
        var keys = await storage.ListOlderThanAsync(PhotoKeys.TemporaryRoot, cutoff, ct);
        foreach (var key in keys)
            await storage.DeleteAsync(key, ct);

        if (keys.Count > 0)
            log.LogInformation("{Count} kullanılmayan geçici fotoğraf silindi", keys.Count);
        return keys.Count;
    }
}
