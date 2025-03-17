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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the User entity
            modelBuilder.Entity<User>()
                .ToTable("users");

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasColumnName("id");

            modelBuilder.Entity<User>()
                .Property(u => u.Auth0Id)
                .HasColumnName("auth0_id");

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasColumnName("username");

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasColumnName("email");

            modelBuilder.Entity<User>()
                .Property(u => u.DisplayName)
                .HasColumnName("display_name");

            modelBuilder.Entity<User>()
                .Property(u => u.ProfilePicture)
                .HasColumnName("profile_picture");

            modelBuilder.Entity<User>()
                .Property(u => u.Bio)
                .HasColumnName("bio");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnName("created_at");

            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt)
                .HasColumnName("updated_at");

            // Add unique constraint for Auth0Id
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Auth0Id)
                .IsUnique();

            // Add unique constraint for Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Add unique constraint for Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
} 