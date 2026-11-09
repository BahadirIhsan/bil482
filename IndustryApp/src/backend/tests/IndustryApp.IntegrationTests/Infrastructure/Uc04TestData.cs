using IndustryApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryApp.IntegrationTests.Infrastructure;

/// <summary>Bir UC-04 senaryosu: müşteri, eşleşmiş ustalar, Open rapor ve her ustadan bir Pending teklif.</summary>
public sealed record Uc04Scenario(Guid CustomerId, Guid ReportId, IReadOnlyList<Guid> MechanicIds, IReadOnlyList<Guid> OfferIds);

/// <summary>
/// Test verisini doğrudan SQL ile yazar (UC-02 ve UC-03 ekranları henüz yok; yerel betik
/// database/dev/uc04-dev-data.sql ile aynı şekil). Her çağrı yeni kimliklerle bağımsız bir senaryo üretir.
/// </summary>
public static class Uc04TestData
{
    public static async Task<Uc04Scenario> CreateAsync(PostgresFixture fixture, int mechanicCount = 3, int matchedOnly = 0)
    {
        var customerId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var mechanics = Enumerable.Range(0, mechanicCount + matchedOnly).Select(_ => Guid.NewGuid()).ToList();
        var offers = new List<Guid>();

        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await InsertUserAsync(db, customerId, "Test Müşteri", "05321234567");
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO customer_profiles (user_id, city, district) VALUES ({customerId}, 'Ankara', 'Çankaya')");
        fixture.Identity.Add(customerId, "Test Müşteri", "05321234567", $"musteri-{customerId:N}@test.local");

        for (var i = 0; i < mechanics.Count; i++)
        {
            var id = mechanics[i];
            var phone = $"0544{i:D7}";
            await InsertUserAsync(db, id, $"Usta {i + 1}", phone);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO mechanic_profiles (user_id, workshop_name, tax_number, city, district, address, latitude, longitude, approval_status, approved_at)
                VALUES ({id}, {"Atölye " + (char)('A' + i)}, {id.ToString("N")[..10]}, 'Ankara', 'Ostim', 'Ostim OSB', 39.97, 32.75, 'Approved', now())
                """);
            fixture.Identity.Add(id, $"Usta {i + 1}", phone, $"usta-{id:N}@test.local");
        }

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO accident_reports (id, customer_id, vehicle_type_id, city, district, latitude, longitude, status, expires_at)
            VALUES ({reportId}, {customerId}, (SELECT id FROM vehicle_types WHERE code = 'CAR'), 'Ankara', 'Çankaya', 39.92, 32.85, 'Open', now() + interval '72 hours')
            """);

        foreach (var mechanicId in mechanics)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO report_matches (report_id, mechanic_id, strategy_code) VALUES ({reportId}, {mechanicId}, 'CITY')");
        }

        // Yalnızca ilk mechanicCount usta teklif verir; kalanlar eşleşmiş ama teklifsizdir.
        for (var i = 0; i < mechanicCount; i++)
        {
            var offerId = Guid.NewGuid();
            var price = 4000m + 500m * i;
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO offers (id, report_id, mechanic_id, total_price, estimated_days, status)
                VALUES ({offerId}, {reportId}, {mechanics[i]}, {price}, {(short)(4 - i)}, 'Pending')
                """);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO offer_price_items (offer_id, item_type, amount) VALUES ({offerId}, 'Labor', {price})");
            offers.Add(offerId);
        }

        return new Uc04Scenario(customerId, reportId, mechanics, offers);
    }

    private static Task<int> InsertUserAsync(AppDbContext db, Guid id, string name, string phone)
    {
        var email = $"{id:N}@test.local";
        return db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO users (id, full_name, user_name, normalized_user_name, email, normalized_email, phone_number)
            VALUES ({id}, {name}, {email}, {email.ToUpperInvariant()}, {email}, {email.ToUpperInvariant()}, {phone})
            """);
    }
}
