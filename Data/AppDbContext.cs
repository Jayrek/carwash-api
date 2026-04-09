using CarwashApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CarwashApi.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<User>(entity => {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
            entity.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(u => u.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(u => u.ProfileImageUrl).HasColumnName("profile_image_url").HasMaxLength(2048);
            entity.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
            entity.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
            entity.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
            entity.Property(u => u.PasswordSalt).HasColumnName("password_salt").HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<UserDevice>(entity => {
            entity.ToTable("user_devices");

            entity.HasKey(d => d.Id);

            entity.Property(d => d.Id).HasColumnName("id");
            entity.Property(d => d.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(d => d.DeviceToken).HasColumnName("device_token").HasMaxLength(512).IsRequired();
            entity.Property(d => d.Platform).HasColumnName("platform").HasMaxLength(32).IsRequired();
            entity.Property(d => d.DeviceId).HasColumnName("device_id").HasMaxLength(128);
            entity.Property(d => d.DeviceName).HasColumnName("device_name").HasMaxLength(256);
            entity.Property(d => d.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
            entity.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
            entity.Property(d => d.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
            entity.Property(d => d.LastUsedAt).HasColumnName("last_used_at");

            entity.HasIndex(d => new { d.UserId, d.DeviceToken }).IsUnique();

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity => {
            entity.ToTable("notifications");

            entity.HasKey(n => n.Id);

            entity.Property(n => n.Id).HasColumnName("id");
            entity.Property(n => n.UserId).HasColumnName("user_id");
            entity.Property(n => n.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(n => n.Body).HasColumnName("body").HasMaxLength(4000).IsRequired();
            entity.Property(n => n.Type).HasColumnName("type").HasMaxLength(64);
            entity.Property(n => n.Data).HasColumnName("data");
            entity.Property(n => n.IsBroadcast).HasColumnName("is_broadcast").HasDefaultValue(false).IsRequired();
            entity.Property(n => n.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
            entity.Property(n => n.SentAt).HasColumnName("sent_at");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(n => n.Deliveries)
                .WithOne(d => d.Notification)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationDelivery>(entity => {
            entity.ToTable("notification_deliveries");

            entity.HasKey(d => d.Id);

            entity.Property(d => d.Id).HasColumnName("id");
            entity.Property(d => d.NotificationId).HasColumnName("notification_id").IsRequired();
            entity.Property(d => d.UserDeviceId).HasColumnName("user_device_id").IsRequired();
            entity.Property(d => d.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
            entity.Property(d => d.Response).HasColumnName("response").HasMaxLength(2048);
            entity.Property(d => d.SentAt).HasColumnName("sent_at").HasDefaultValueSql("now()").IsRequired();

            entity.HasIndex(d => new { d.NotificationId, d.UserDeviceId }).IsUnique();

            entity.HasOne(d => d.UserDevice)
                .WithMany()
                .HasForeignKey(d => d.UserDeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
