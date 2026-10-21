using IndustryApp.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryApp.Infrastructure.Persistence.Configurations;

/// <summary>damage_categories tablosu (T-6.4): kendi kendine 1–N ağaç (Composite).</summary>
public sealed class DamageCategoryConfiguration : IEntityTypeConfiguration<DamageCategory>
{
    public void Configure(EntityTypeBuilder<DamageCategory> b)
    {
        b.ToTable("damage_categories", t => t.HasCheckConstraint(
            "ck_damage_categories_level", $"level BETWEEN 1 AND {DamageCategory.MaxLevel}"));
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.SortOrder).HasDefaultValue((short)0);
        b.Property(x => x.IsActive).HasDefaultValue(true);

        b.HasMany(x => x.SubCategories).WithOne().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.NoAction);
        b.Navigation(x => x.SubCategories).HasField("_children");
        b.HasOne<DamageCategory>().WithMany().HasForeignKey(x => x.RootId).OnDelete(DeleteBehavior.NoAction);

        b.HasIndex(x => x.ParentId).HasDatabaseName("ix_damage_categories_parent");
    }
}

/// <summary>vehicle_types tablosu (T-6.4).</summary>
public sealed class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
{
    public void Configure(EntityTypeBuilder<VehicleType> b)
    {
        b.ToTable("vehicle_types");
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.Code).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).HasMaxLength(60).IsRequired();
        b.Property(x => x.SortOrder).HasDefaultValue((short)0);
        b.Property(x => x.IsActive).HasDefaultValue(true);
    }
}
