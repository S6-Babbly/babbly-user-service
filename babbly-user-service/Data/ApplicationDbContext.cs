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
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.Auth0Id).HasColumnName("auth0_id").IsRequired();
                entity.Property(e => e.Username).HasColumnName("username").IsRequired();
                entity.Property(e => e.Role).HasColumnName("role").IsRequired();
                // FirstName and LastName columns don't exist in current schema
                // entity.Property(e => e.FirstName).HasColumnName("first_name");
                // entity.Property(e => e.LastName).HasColumnName("last_name");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.HasIndex(e => e.Auth0Id).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.ToTable("users");
            });

            // Configure UserExtraData entity
            modelBuilder.Entity<UserExtraData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.DisplayName).HasColumnName("display_name");
                entity.Property(e => e.ProfilePicture).HasColumnName("profile_picture");
                entity.Property(e => e.Bio).HasColumnName("bio");
                entity.Property(e => e.Address).HasColumnName("address");
                entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.HasOne(e => e.User)
                      .WithOne(u => u.ExtraData)
                      .HasForeignKey<UserExtraData>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.ToTable("user_extra_data");
            });
        }
    }
}