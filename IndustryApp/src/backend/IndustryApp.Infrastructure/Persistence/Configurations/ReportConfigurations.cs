using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Catalog;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using IndustryApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static IndustryApp.Infrastructure.Persistence.Configurations.ConfigurationExtensions;

namespace IndustryApp.Infrastructure.Persistence.Configurations;

/// <summary>accident_reports tablosu (T-6.5).</summary>
public sealed class AccidentReportConfiguration : IEntityTypeConfiguration<AccidentReport>
{
    public void Configure(EntityTypeBuilder<AccidentReport> b)
    {
        b.ToTable("accident_reports", t =>
        {
            t.HasCheckConstraint("ck_accident_reports_status", InCheck<ReportStatus>("status"));
            t.HasCheckConstraint("ck_accident_reports_selected_offer", "status <> 'OfferSelected' OR selected_offer_id IS NOT NULL");
        });
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.City).HasMaxLength(50).IsRequired();
        b.Property(x => x.District).HasMaxLength(80);
        b.Property(x => x.AddressText).HasMaxLength(300);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(ReportStatus.Open);
        b.Property(x => x.CreatedAt).HasDefaultValueSql(Now);
        b.UseXminConcurrencyToken();

        b.HasOne<CustomerProfile>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne<VehicleType>().WithMany().HasForeignKey(x => x.VehicleTypeId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne<Offer>().WithMany().HasForeignKey(x => x.SelectedOfferId).IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction).HasConstraintName("fk_accident_reports_selected_offer");

        b.HasMany(x => x.DamageItems).WithOne().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Photos).WithOne().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Matches).WithOne().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Offers).WithOne().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.NoAction);

        b.HasIndex(x => new { x.CustomerId, x.CreatedAt }).IsDescending(false, true).HasDatabaseName("ix_accident_reports_customer");
        b.HasIndex(x => x.ExpiresAt).HasFilter("status = 'Open'").HasDatabaseName("ix_accident_reports_expiry");
    }
}

/// <summary>report_damage_items tablosu (T-6.5).</summary>
public sealed class ReportDamageItemConfiguration : IEntityTypeConfiguration<ReportDamageItem>
{
    public void Configure(EntityTypeBuilder<ReportDamageItem> b)
    {
        b.ToTable("report_damage_items", t => t.HasCheckConstraint("ck_report_damage_items_severity", InCheck<DamageSeverity>("severity")));
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(10);
        b.Property(x => x.Note).HasMaxLength(300);
        b.HasOne<DamageCategory>().WithMany().HasForeignKey(x => x.DamageCategoryId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(x => new { x.ReportId, x.DamageCategoryId }).IsUnique();
    }
}

/// <summary>report_photos tablosu (T-6.5). Dosyanın kendisi IFileStorage'dadır.</summary>
public sealed class ReportPhotoConfiguration : IEntityTypeConfiguration<ReportPhoto>
{
    public void Configure(EntityTypeBuilder<ReportPhoto> b)
    {
        b.ToTable("report_photos");
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.StorageKey).HasMaxLength(300).IsRequired();
        b.Property(x => x.OriginalFileName).HasMaxLength(255);
        b.Property(x => x.ContentType).HasMaxLength(50).IsRequired();
        b.Property(x => x.SortOrder).HasDefaultValue((short)0);
        b.Property(x => x.CreatedAt).HasDefaultValueSql(Now);
    }
}

/// <summary>report_matches tablosu (T-6.5): rapor ile usta arasında N–N.</summary>
public sealed class ReportMatchConfiguration : IEntityTypeConfiguration<ReportMatch>
{
    public void Configure(EntityTypeBuilder<ReportMatch> b)
    {
        b.ToTable("report_matches");
        b.HasKey(x => new { x.ReportId, x.MechanicId });
        b.Property(x => x.StrategyCode).HasMaxLength(30).IsRequired();
        b.Property(x => x.DistanceKm).HasPrecision(6, 2);
        b.Property(x => x.NotifiedAt).HasDefaultValueSql(Now);
        b.HasOne<MechanicProfile>().WithMany().HasForeignKey(x => x.MechanicId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(x => new { x.MechanicId, x.NotifiedAt }).IsDescending(false, true).HasDatabaseName("ix_report_matches_mechanic");
    }
}

/// <summary>report_status_history tablosu (T-6.5): durum geçmişi ve geri alma snapshot'ı.</summary>
public sealed class ReportStatusHistoryConfiguration : IEntityTypeConfiguration<ReportStatusHistory>
{
    public void Configure(EntityTypeBuilder<ReportStatusHistory> b)
    {
        b.ToTable("report_status_history");
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CommandName).HasMaxLength(60).IsRequired();
        b.Property(x => x.Snapshot).HasColumnType("jsonb");
        b.Property(x => x.OccurredAt).HasDefaultValueSql(Now);
        b.HasOne<AccidentReport>().WithMany().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(x => new { x.ReportId, x.OccurredAt }).IsDescending(false, true).HasDatabaseName("ix_report_status_history_report");
    }
}
