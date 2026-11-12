using FluentAssertions;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Common.Options;
using IndustryApp.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IndustryApp.UnitTests.Infrastructure;

/// <summary>Rapora bağlanmayan geçici fotoğraflar saklama süresinden sonra silinir.</summary>
public sealed class TemporaryUploadCleanupJobTests
{
    [Fact]
    public async Task RunOnce_DeletesTemporaryFilesOlderThanRetention()
    {
        var now = new DateTimeOffset(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);
        var storage = Substitute.For<IFileStorage>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(now);
        storage.ListOlderThanAsync("temp", now.AddHours(-24), Arg.Any<CancellationToken>())
            .Returns(["temp/u1/a.jpg", "temp/u2/b.jpg"]);
        var job = new TemporaryUploadCleanupJob(storage, clock,
            Options.Create(new PhotosOptions { TemporaryRetentionHours = 24 }), NullLogger<TemporaryUploadCleanupJob>.Instance);

        var deleted = await job.RunOnceAsync(CancellationToken.None);

        deleted.Should().Be(2);
        await storage.Received(1).DeleteAsync("temp/u1/a.jpg", Arg.Any<CancellationToken>());
        await storage.Received(1).DeleteAsync("temp/u2/b.jpg", Arg.Any<CancellationToken>());
    }
}
