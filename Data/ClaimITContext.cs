// Data/ClaimITContext.cs
using Microsoft.EntityFrameworkCore;
using ClaimIT.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClaimIT.Data
{
    public class ClaimITContext : DbContext
    {
        public ClaimITContext(DbContextOptions<ClaimITContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; } = null!;
        public DbSet<ClaimAudit> ClaimAudits { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Add this to ignore the pending changes warning temporarily
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Your existing configuration here...
            // Decimal precision
            modelBuilder.Entity<Claim>()
                .Property(c => c.HoursWorked)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Claim>()
                .Property(c => c.HourlyRate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<User>()
                .Property(u => u.HourlyRate)
                .HasColumnType("decimal(18,2)");

            // Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FullName = "John Lecturer", Email = "lecturer@university.com", PasswordHash = "lecturer123", Role = "Lecturer", HourlyRate = 680.00m, Department = "Computer Science", IsActive = true },
                new User { Id = 2, FullName = "Sarah Coordinator", Email = "coordinator@university.com", PasswordHash = "coordinator123", Role = "Coordinator", HourlyRate = 0m, Department = "Administration", IsActive = true },
                new User { Id = 3, FullName = "Mike Manager", Email = "manager@university.com", PasswordHash = "manager123", Role = "Manager", HourlyRate = 0m, Department = "Finance", IsActive = true },
                new User { Id = 4, FullName = "Lisa HR", Email = "hr@university.com", PasswordHash = "hr123", Role = "HR", HourlyRate = 0m, Department = "Human Resources", IsActive = true }
            );

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            ClaimITSeeder.Seed(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
    }
}