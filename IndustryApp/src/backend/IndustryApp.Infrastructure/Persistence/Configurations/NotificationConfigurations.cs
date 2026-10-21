using IndustryApp.Domain.Notifications;
using IndustryApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static IndustryApp.Infrastructure.Persistence.Configurations.ConfigurationExtensions;

namespace IndustryApp.Infrastructure.Persistence.Configurations;

/// <summary>notifications tablosu (T-6.6).</summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("notifications");
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Type).HasMaxLength(40).IsRequired();
        b.Property(x => x.Title).HasMaxLength(150).IsRequired();
        b.Property(x => x.Body).HasMaxLength(500).IsRequired();
        b.Property(x => x.Payload).HasColumnType("jsonb");
        b.Property(x => x.IsRead).HasDefaultValue(false);
        b.Property(x => x.CreatedAt).HasDefaultValueSql(Now);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt }).IsDescending(false, false, true)
            .HasDatabaseName("ix_notifications_user_unread");
    }
}

/// <summary>notification_deliveries tablosu (T-6.6): kanal başına gönderim sonucu, Circuit Breaker izleri.</summary>
public sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> b)
    {
        b.ToTable("notification_deliveries", t =>
        {
            t.HasCheckConstraint("ck_notification_deliveries_channel", InCheck<NotificationChannelType>("channel"));
            t.HasCheckConstraint("ck_notification_deliveries_status", InCheck<NotificationDeliveryStatus>("status"));
        });
        b.Property(x => x.Id).UseIdentityByDefaultColumn();
        b.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.AttemptCount).HasDefaultValue((short)1);
        b.Property(x => x.LastError).HasMaxLength(NotificationDelivery.MaxErrorLength);
        b.Property(x => x.CreatedAt).HasDefaultValueSql(Now);
        b.HasOne<Notification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
    }
}
