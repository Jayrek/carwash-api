using CarwashApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CarwashApi.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<User>(entity => 
            {
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
        }
}