using FluentAssertions;
using IndustryApp.Application.Abstractions.Repositories;
using IndustryApp.Application.Catalog;
using IndustryApp.Application.Common.Options;
using IndustryApp.Domain.Catalog;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace IndustryApp.UnitTests.Application.Catalog;

/// <summary>NFR-P1: hasar ağacı önbellekten gelir; admin değişikliği anahtarı temizleyince yeniden okunur.</summary>
public sealed class CatalogServiceTests
{
    private readonly IDamageCategoryRepository _categories = Substitute.For<IDamageCategoryRepository>();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly CatalogService _service;

    public CatalogServiceTests()
    {
        var body = DamageCategory.Create("BODY", "Kaporta", 1).WithId(1);
        DamageCategory.Create("BODY_BUMPER", "Ön tampon", 1, body).WithId(2);
        _categories.GetTreeAsync(false, Arg.Any<CancellationToken>()).Returns([body]);

        _service = new CatalogService(_categories, Substitute.For<IVehicleTypeRepository>(), Substitute.For<ILegalDocumentRepository>(),
            _cache, Options.Create(new CatalogOptions { CacheMinutes = 10 }));
    }

    [Fact]
    public async Task DamageTree_SecondCall_IsServedFromCache()
    {
        var first = await _service.GetDamageTreeAsync(CancellationToken.None);
        var second = await _service.GetDamageTreeAsync(CancellationToken.None);

        second.Should().BeSameAs(first);
        first.Single().Children.Single().Name.Should().Be("Ön tampon");
        await _categories.Received(1).GetTreeAsync(false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DamageTree_AfterCacheKeyRemoved_IsReloaded()
    {
        await _service.GetDamageTreeAsync(CancellationToken.None);

        _cache.Remove(CatalogCacheKeys.DamageTree);
        await _service.GetDamageTreeAsync(CancellationToken.None);

        await _categories.Received(2).GetTreeAsync(false, Arg.Any<CancellationToken>());
    }
}
