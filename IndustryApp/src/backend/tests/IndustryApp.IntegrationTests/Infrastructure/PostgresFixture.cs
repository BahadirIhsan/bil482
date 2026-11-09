using IndustryApp.Application;
using IndustryApp.Application.Abstractions;
using IndustryApp.Application.Common.Options;
using IndustryApp.Infrastructure;
using IndustryApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace IndustryApp.IntegrationTests.Infrastructure;

/// <summary>
/// Gerçek PostgreSQL 16 konteyneri: migration + database/reference-data.sql ile kurulur (T-19.1).
/// Servisler Application + Infrastructure DI kayıtlarıyla gerçek repository ve UnitOfWork kullanır;
/// yalnızca kimlik tarafı (ICurrentUser, IIdentityService) test sürümleriyle değiştirilir.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("industryapp")
        .WithUsername("industryapp")
        .WithPassword("test")
        .Build();

    public ServiceProvider Services { get; private set; } = default!;

    public TestIdentityService Identity { get; } = new();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _container.GetConnectionString(),
                ["Offers:SelectionUndoWindowMinutes"] = "10"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<OffersOptions>().Bind(configuration.GetSection(OffersOptions.SectionName));
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddScoped<TestCurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<TestCurrentUser>());
        services.AddSingleton<IIdentityService>(Identity);
        Services = services.BuildServiceProvider();

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await db.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync(FindRepoFile("database", "reference-data.sql")));
    }

    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();
        await _container.DisposeAsync();
    }

    /// <summary>Verilen kullanıcı adına çalışan yeni bir DI kapsamı (bir HTTP isteğinin karşılığı).</summary>
    public AsyncServiceScope ScopeFor(Guid userId)
    {
        var scope = Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<TestCurrentUser>().UserId = userId;
        return scope;
    }

    private static string FindRepoFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine([dir.FullName, .. parts]);
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException($"{Path.Combine(parts)} bulunamadı.");
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "postgres";
}
