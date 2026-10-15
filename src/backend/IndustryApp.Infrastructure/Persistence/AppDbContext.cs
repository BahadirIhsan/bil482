using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Catalog;
using IndustryApp.Domain.Common;
using IndustryApp.Domain.Notifications;
using IndustryApp.Domain.Offers;
using IndustryApp.Domain.Reports;
using IndustryApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IndustryApp.Infrastructure.Persistence;

/// <summary>
/// Hem Identity tablolarını hem uygulama tablolarını yönetir. Şema T-6'daki DDL'i hedefler;
/// isimler EFCore.NamingConventions ile snake_case'e çevrilir.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();

    public DbSet<MechanicProfile> MechanicProfiles => Set<MechanicProfile>();

    public DbSet<MechanicSpecialty> MechanicSpecialties => Set<MechanicSpecialty>();

    public DbSet<DamageCategory> DamageCategories => Set<DamageCategory>();

    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();

    public DbSet<LegalDocument> LegalDocuments => Set<LegalDocument>();

    public DbSet<UserConsent> UserConsents => Set<UserConsent>();

    public DbSet<AccidentReport> AccidentReports => Set<AccidentReport>();

    public DbSet<ReportDamageItem> ReportDamageItems => Set<ReportDamageItem>();

    public DbSet<ReportPhoto> ReportPhotos => Set<ReportPhoto>();

    public DbSet<ReportMatch> ReportMatches => Set<ReportMatch>();

    public DbSet<ReportStatusHistory> ReportStatusHistory => Set<ReportStatusHistory>();

    public DbSet<Offer> Offers => Set<Offer>();

    public DbSet<OfferPriceItem> OfferPriceItems => Set<OfferPriceItem>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<ApplicationUser>().ToTable("users");
        b.Entity<ApplicationRole>().ToTable("roles");
        b.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        b.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        b.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        b.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        b.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");

        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in b.Model.GetEntityTypes().ToList())
        {
            // Olay listesi kalıcı değildir; UnitOfWork kayıttan sonra okur ve temizler.
            if (typeof(IHasDomainEvents).IsAssignableFrom(entityType.ClrType))
                b.Entity(entityType.ClrType).Ignore(nameof(IHasDomainEvents.DomainEvents));

            // DDL'deki DEFAULT değerleri yalnızca elle yazılan SQL (referans veri, yerel betikler) içindir.
            // Uygulama her değeri kendisi yazar; aksi halde "false" gibi CLR varsayılanları DEFAULT true ile ezilirdi.
            foreach (var property in entityType.GetProperties())
            {
                if (property.FindAnnotation(RelationalAnnotationNames.DefaultValue) is not null
                    || property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql) is not null)
                    property.ValueGenerated = ValueGenerated.Never;
            }
        }
    }
}
