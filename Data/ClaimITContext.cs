// Data/ClaimITContext.cs
using Microsoft.EntityFrameworkCore;
using ClaimIT.Models;          // ← THIS LINE WAS MISSING — THIS FIXES EVERYTHING

namespace ClaimIT.Data
{
    public class ClaimITContext : DbContext
    {
        public ClaimITContext(DbContextOptions<ClaimITContext> options) : base(options) { }

        public DbSet<Claim> Claims { get; set; } = null!;
        public DbSet<ClaimAudit> ClaimAudits { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed the 4 demo users — now works perfectly
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "John Lecturer",
                    Email = "lecturer@university.com",
                    PasswordHash = "lecturer123",
                    Role = "Lecturer",
                    HourlyRate = 280.00m,
                    Department = "Computer Science",
                    IsActive = true
                },
                new User
                {
                    Id = 2,
                    FullName = "Sarah Coordinator",
                    Email = "coordinator@university.com",
                    PasswordHash = "coordinator123",
                    Role = "Coordinator",
                    Department = "Administration",
                    HourlyRate = 0m,
                    IsActive = true
                },
                new User
                {
                    Id = 3,
                    FullName = "Mike Manager",
                    Email = "manager@university.com",
                    PasswordHash = "manager123",
                    Role = "Manager",
                    Department = "Finance",
                    HourlyRate = 0m,
                    IsActive = true
                },
                new User
                {
                    Id = 4,
                    FullName = "Lisa HR",
                    Email = "hr@university.com",
                    PasswordHash = "hr123",
                    Role = "HR",
                    Department = "Human Resources",
                    HourlyRate = 0m,
                    IsActive = true
                }
            );

            // Optional: Ensure unique emails
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}