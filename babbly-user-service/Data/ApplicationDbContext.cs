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
                .Property(u => u.Role)
                .HasColumnName("role");

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

            // Configure the UserExtraData entity
            modelBuilder.Entity<UserExtraData>()
                .ToTable("user_extra_data");

            modelBuilder.Entity<UserExtraData>()
                .Property(u => u.Id)
                .HasColumnName("id");

            modelBuilder.Entity<UserExtraData>()
                .Property(u => u.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<UserExtraData>()
                .Property(u => u.DisplayName)
                .HasColumnName("display_name");

            modelBuilder.Entity<UserExtraData>()
                .Property(u => u.ProfilePicture)
                .HasColumnName("profile_picture");

            modelBuilder.Entity<UserExtraData>()
                .Property(u => u.Bio)
                .HasColumnName("bio");

            modelBuilder.Entity<UserExtraData>()
                .Property(u => u.Address)
                .HasColumnName("address");

            modelBuilder.Entity<UserExtraData>()
                .Property(u => u.PhoneNumber)
                .HasColumnName("phone_number");

            modelBuilder.Entity<UserExtraData>()
                .Property(u => u.CreatedAt)
                .HasColumnName("created_at");

            modelBuilder.Entity<UserExtraData>()
                .Property(u => u.UpdatedAt)
                .HasColumnName("updated_at");

            // Configure one-to-one relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.ExtraData)
                .WithOne(e => e.User)
                .HasForeignKey<UserExtraData>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 