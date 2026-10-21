using IndustryApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static IndustryApp.Infrastructure.Persistence.Configurations.ConfigurationExtensions;

namespace IndustryApp.Infrastructure.Persistence.Configurations;

/// <summary>users tablosu (T-6.3): Identity kolonları + IndustryApp eklemeleri.</summary>
public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.Property(x => x.UserName).HasMaxLength(256);
        b.Property(x => x.NormalizedUserName).HasMaxLength(256);
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
        b.HasIndex(x => x.NormalizedEmail).IsUnique();
        b.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();

        b.Property(x => x.EmailConfirmed).HasDefaultValue(false);
        b.Property(x => x.PhoneNumberConfirmed).HasDefaultValue(false);
        b.Property(x => x.TwoFactorEnabled).HasDefaultValue(false);
        b.Property(x => x.LockoutEnabled).HasDefaultValue(true);
        b.Property(x => x.AccessFailedCount).HasDefaultValue(0);

        b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.CreatedAt).HasDefaultValueSql(Now);
    }
}

/// <summary>roles tablosu (T-6.3). Satırlar reference-data.sql ile gelir.</summary>
public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> b)
    {
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.NormalizedName).HasMaxLength(256).IsRequired();
    }
}

/// <summary>refresh_tokens tablosu (T-6.3): yalnızca SHA-256 özeti saklanır.</summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.Property(x => x.CreatedAt).HasDefaultValueSql(Now);
        b.Property(x => x.CreatedByIp).HasMaxLength(45);
        b.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
