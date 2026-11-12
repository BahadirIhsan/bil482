using System.Text;
using FluentAssertions;
using IndustryApp.Application.Common.Options;
using IndustryApp.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace IndustryApp.UnitTests.Infrastructure;

/// <summary>PAT-02.4 Adapter: anahtar → disk yolu, kök dışına çıkış engellenir.</summary>
public sealed class LocalFileStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "industryapp-tests", Guid.NewGuid().ToString("N"));
    private readonly LocalFileStorage _storage;

    public LocalFileStorageTests() =>
        _storage = new LocalFileStorage(Options.Create(new StorageOptions { RootPath = _root }));

    // TC-02.1.2
    [Theory]
    [InlineData("../outside.jpg")]
    [InlineData("temp/../../outside.jpg")]
    [InlineData("/etc/passwd")]
    [InlineData("temp/./x.jpg")]
    [InlineData("temp\\..\\x.jpg")]
    [InlineData("temp//x.jpg")]
    [InlineData("")]
    public async Task LocalFileStorage_PathTraversal_Throws(string key)
    {
        var act = () => _storage.SaveAsync(new MemoryStream([1, 2, 3]), key, "image/jpeg", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        Directory.GetFiles(Path.GetDirectoryName(_root)!, "outside.jpg", SearchOption.AllDirectories).Should().BeEmpty();
    }

    [Fact]
    public async Task SaveMoveOpen_RoundTrip()
    {
        await _storage.SaveAsync(new MemoryStream("abc"u8.ToArray()), "temp/u1/a.jpg", "image/jpeg", CancellationToken.None);

        await _storage.MoveAsync("temp/u1/a.jpg", "reports/2026/11/r1/p1.jpg", CancellationToken.None);

        (await _storage.GetSizeAsync("temp/u1/a.jpg", CancellationToken.None)).Should().BeNull();
        (await _storage.GetSizeAsync("reports/2026/11/r1/p1.jpg", CancellationToken.None)).Should().Be(3);
        await using var stream = await _storage.OpenReadAsync("reports/2026/11/r1/p1.jpg", CancellationToken.None);
        new StreamReader(stream!, Encoding.UTF8).ReadToEnd().Should().Be("abc");
    }

    [Fact]
    public async Task Move_MissingSource_ThrowsFileNotFound()
    {
        var act = () => _storage.MoveAsync("temp/u1/missing.jpg", "reports/x.jpg", CancellationToken.None);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task ListOlderThan_ReturnsOnlyOldFilesUnderPrefix()
    {
        await _storage.SaveAsync(new MemoryStream([1]), "temp/u1/old.jpg", "image/jpeg", CancellationToken.None);
        await _storage.SaveAsync(new MemoryStream([1]), "temp/u1/new.jpg", "image/jpeg", CancellationToken.None);
        await _storage.SaveAsync(new MemoryStream([1]), "reports/2026/11/r/p.jpg", "image/jpeg", CancellationToken.None);
        File.SetLastWriteTimeUtc(Path.Combine(_root, "temp", "u1", "old.jpg"), DateTime.UtcNow.AddDays(-2));
        File.SetLastWriteTimeUtc(Path.Combine(_root, "reports", "2026", "11", "r", "p.jpg"), DateTime.UtcNow.AddDays(-2));

        var keys = await _storage.ListOlderThanAsync("temp", DateTimeOffset.UtcNow.AddHours(-24), CancellationToken.None);

        keys.Should().Equal("temp/u1/old.jpg");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
