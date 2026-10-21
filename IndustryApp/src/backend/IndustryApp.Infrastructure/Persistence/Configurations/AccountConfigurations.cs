using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Catalog;
using IndustryApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static IndustryApp.Infrastructure.Persistence.Configurations.ConfigurationExtensions;

namespace IndustryApp.Infrastructure.Persistence.Configurations;

/// <summary>customer_profiles tablosu (T-6.3): users 1–1.</summary>
public sealed class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> b)
    {
        b.ToTable("customer_profiles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("user_id").ValueGeneratedNever();
        b.Property(x => x.City).HasMaxLength(50).IsRequired();
        b.Property(x => x.District).HasMaxLength(80);
        b.Property(x => x.CreatedAt).HasDefaultValueSql(Now);
        b.HasOne<ApplicationUser>().WithOne().HasForeignKey<CustomerProfile>(x => x.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>mechanic_profiles tablosu (T-6.3): users 1–1, onay durumu ve eşleştirme alanları.</summary>
public sealed class MechanicProfileConfiguration : IEntityTypeConfiguration<MechanicProfile>
{
    public void Configure(EntityTypeBuilder<MechanicProfile> b)
    {
        b.ToTable("mechanic_profiles", t =>
        {
            t.HasCheckConstraint("ck_mechanic_profiles_latitude", "latitude BETWEEN -90 AND 90");
            t.HasCheckConstraint("ck_mechanic_profiles_longitude", "longitude BETWEEN -180 AND 180");
            t.HasCheckConstraint("ck_mechanic_profiles_approval_status", InCheck<MechanicApprovalStatus>("approval_status"));
            t.HasCheckConstraint("ck_mechanic_profiles_daily_capacity", "daily_capacity > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("user_id").ValueGeneratedNever();
        b.Property(x => x.WorkshopName).HasMaxLength(150).IsRequired();
        b.Property(x => x.TaxNumber).HasMaxLength(11).IsRequired();
        b.HasIndex(x => x.TaxNumber).IsUnique();
        b.Property(x => x.City).HasMaxLength(50).IsRequired();
        b.Property(x => x.District).HasMaxLength(80).IsRequired();
        b.Property(x => x.Address).HasMaxLength(300).IsRequired();
        b.Property(x => x.ApprovalStatus).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(MechanicApprovalStatus.PendingApproval);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.IsAvailable).HasDefaultValue(true);
        b.Property(x => x.DailyCapacity).HasDefaultValue(MechanicProfile.DefaultDailyCapacity);
        b.Property(x => x.CreatedAt).HasDefaultValueSql(Now);

        b.HasOne<ApplicationUser>().WithOne().HasForeignKey<MechanicProfile>(x => x.Id).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ApprovedBy).OnDelete(DeleteBehavior.NoAction);
        b.HasMany(x => x.Specialties).WithOne().HasForeignKey(x => x.MechanicId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.City, x.ApprovalStatus, x.IsAvailable }).HasDatabaseName("ix_mechanic_profiles_matching");
    }
}

/// <summary>mechanic_specialties tablosu (T-6.4): yalnızca 1. seviye kategoriler.</summary>
public sealed class MechanicSpecialtyConfiguration : IEntityTypeConfiguration<MechanicSpecialty>
{
    public void Configure(EntityTypeBuilder<MechanicSpecialty> b)
    {
        b.ToTable("mechanic_specialties");
        b.HasKey(x => new { x.MechanicId, x.DamageCategoryId });
        b.HasOne<DamageCategory>().WithMany().HasForeignKey(x => x.DamageCategoryId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(x => x.DamageCategoryId).HasDatabaseName("ix_mechanic_specialties_category");
    }
}

/// <summary>legal_documents tablosu (T-6.4): kod başına tek güncel sürüm.</summary>
public sealed class LegalDocumentConfiguration : IEntityTypeConfiguration<LegalDocument>
{
    public void Configure(EntityTypeBuilder<LegalDocument> b)
    {
        b.ToTable("legal_documents", t => t.HasCheckConstraint(
            "ck_legal_documents_target_role",
            $"target_role IN ('{RoleNames.Customer}','{RoleNames.Mechanic}','{LegalDocument.AllRoles}')"));
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Version).HasMaxLength(20).IsRequired();
        b.Property(x => x.TargetRole).HasMaxLength(20).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Body).HasColumnType("text").IsRequired();
        b.Property(x => x.PublishedAt).HasDefaultValueSql(Now);
        b.Property(x => x.IsCurrent).HasDefaultValue(true);
        b.HasIndex(x => new { x.Code, x.Version }).IsUnique();
        b.HasIndex(x => x.Code).IsUnique().HasFilter("is_current").HasDatabaseName("ux_legal_documents_current");
    }
}

/// <summary>user_consents tablosu (T-6.4): onaylanan sözleşme sürümü ve IP (KVKK).</summary>
public sealed class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> b)
    {
        b.ToTable("user_consents");
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.AcceptedAt).HasDefaultValueSql(Now);
        b.Property(x => x.IpAddress).HasMaxLength(45);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<LegalDocument>().WithMany().HasForeignKey(x => x.LegalDocumentId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(x => new { x.UserId, x.LegalDocumentId }).IsUnique();
    }
}
