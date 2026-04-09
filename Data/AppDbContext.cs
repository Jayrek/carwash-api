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
                entity.HasKey(u => u.Id);

                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Phone).HasMaxLength(20);
                entity.Property(u => u.Role).HasMaxLength(50).IsRequired();
                entity.Property(u => u.ProfileImageUrl).HasMaxLength(2048);
                entity.Property(u => u.IsActive).HasDefaultValue(true).IsRequired();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("now()").IsRequired();
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");

                entity.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
                entity.Property(u => u.PasswordSalt).HasMaxLength(256).IsRequired();
            });
        }
}