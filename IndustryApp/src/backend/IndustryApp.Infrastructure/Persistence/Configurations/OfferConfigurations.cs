using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static IndustryApp.Infrastructure.Persistence.Configurations.ConfigurationExtensions;

namespace IndustryApp.Infrastructure.Persistence.Configurations;

/// <summary>offers tablosu (T-6.6). Kısmi unique index bir raporda ikinci kabulü veritabanı seviyesinde engeller (NFR-R1).</summary>
public sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> b)
    {
        b.ToTable("offers", t =>
        {
            t.HasCheckConstraint("ck_offers_total_price", "total_price > 0");
            t.HasCheckConstraint("ck_offers_estimated_days", "estimated_days > 0");
            t.HasCheckConstraint("ck_offers_status", InCheck<OfferStatus>("status"));
        });
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.TotalPrice).HasPrecision(12, 2);
        b.Property(x => x.Currency).HasMaxLength(3).IsFixedLength().HasDefaultValue(Offer.DefaultCurrency);
        b.Property(x => x.Note).HasMaxLength(1000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(OfferStatus.Pending);
        b.Property(x => x.CreatedAt).HasDefaultValueSql(Now);
        b.UseXminConcurrencyToken();

        b.HasOne<MechanicProfile>().WithMany().HasForeignKey(x => x.MechanicId).OnDelete(DeleteBehavior.NoAction);
        b.HasMany(x => x.PriceItems).WithOne().HasForeignKey(x => x.OfferId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.ReportId, x.MechanicId }).IsUnique();
        b.HasIndex(x => x.ReportId).IsUnique()
            .HasFilter("status = 'Accepted'").HasDatabaseName("ux_offers_one_accepted_per_report");
        b.HasIndex(x => new { x.ReportId, x.Status }).HasDatabaseName("ix_offers_report_status");
        b.HasIndex(x => new { x.MechanicId, x.Status, x.CreatedAt }).IsDescending(false, false, true)
            .HasDatabaseName("ix_offers_mechanic_status");
    }
}

/// <summary>offer_price_items tablosu (T-6.6): Decorator zincirinin satırları.</summary>
public sealed class OfferPriceItemConfiguration : IEntityTypeConfiguration<OfferPriceItem>
{
    public void Configure(EntityTypeBuilder<OfferPriceItem> b)
    {
        b.ToTable("offer_price_items", t =>
        {
            t.HasCheckConstraint("ck_offer_price_items_item_type", InCheck<PriceItemType>("item_type"));
            t.HasCheckConstraint("ck_offer_price_items_amount", "amount >= 0");
        });
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.ItemType).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Description).HasMaxLength(200);
        b.Property(x => x.Amount).HasPrecision(12, 2);
        b.Property(x => x.SortOrder).HasDefaultValue((short)0);
    }
}
