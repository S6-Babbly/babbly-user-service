using babbly_user_service.Models;
using Microsoft.EntityFrameworkCore;

namespace babbly_user_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserExtraData> UserExtraData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Auth0Id).IsRequired();
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.HasIndex(e => e.Auth0Id).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.ToTable("users");
            });

            // Configure UserExtraData entity
            modelBuilder.Entity<UserExtraData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.ExtraData)
                      .HasForeignKey<UserExtraData>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.ToTable("user_extra_data");
            });
        }
    }
}