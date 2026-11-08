using IndustryApp.Domain.Catalog;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Offers.Pricing;
using IndustryApp.Domain.Reports;

namespace IndustryApp.UnitTests.Domain;

/// <summary>Domain testlerinde ortak kullanılan sabit zaman ve geçerli nesne üreticileri.</summary>
internal static class DomainTestData
{
    public static readonly DateTimeOffset Now = new(2026, 10, 20, 10, 0, 0, TimeSpan.FromHours(3));

    public static readonly TimeSpan ReportTtl = TimeSpan.FromHours(72);

    public static readonly PhotoMetadata Photo = new("image/jpeg", 250_000, 1600, 1200, "on-tampon.jpg");

    public static DamageCategory Category() => DamageCategory.Create("BODY_FRONT_BUMPER", "Ön tampon", 1);

    /// <summary>Tüm zorunlu adımları doldurulmuş builder; testler gerekirse üzerine ekleme yapar.</summary>
    public static AccidentReportBuilder CompleteBuilder(Guid? customerId = null) =>
        new AccidentReportBuilder()
            .WithCustomer(customerId ?? Guid.NewGuid())
            .WithVehicleType(1)
            .AtLocation("Ankara", "Çankaya", 39.92, 32.85)
            .AddDamage(Category(), DamageSeverity.Medium)
            .AddPhoto(Guid.NewGuid(), "reports/2026/10/test/photo-1.jpg", Photo);

    public static AccidentReport OpenReport(Guid? customerId = null) => CompleteBuilder(customerId).Build(Now, ReportTtl);

    public static Offer OfferFor(AccidentReport report, Guid mechanicId, decimal labor = 3500m) =>
        Offer.Create(report, mechanicId, new BasePrice(labor, "İşçilik"), estimatedDays: 3, note: null, Now);
}
