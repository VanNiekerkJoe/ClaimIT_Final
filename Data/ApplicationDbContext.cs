using Microsoft.EntityFrameworkCore;
using ClaimIT.Models;

namespace ClaimIT.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; } = null!;
        public DbSet<ClaimDocument> ClaimDocuments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed initial data
            modelBuilder.Entity<Claim>().HasData(
                new Claim { Id = 1, LecturerName = "Dr. Sarah Smith", HoursWorked = 40, HourlyRate = 150, Notes = "Monthly teaching hours", Status = "Pending", SubmittedDate = DateTime.Now.AddDays(-2) },
                new Claim { Id = 2, LecturerName = "Prof. James Johnson", HoursWorked = 35, HourlyRate = 180, Notes = "Research supervision", Status = "Verified", SubmittedDate = DateTime.Now.AddDays(-1) },
                new Claim { Id = 3, LecturerName = "Dr. Maria Brown", HoursWorked = 45, HourlyRate = 160, Notes = "Course development", Status = "Approved", SubmittedDate = DateTime.Now.AddDays(-3), ApprovedDate = DateTime.Now.AddDays(-1), ApprovedBy = "Coordinator" }
            );
        }
    }
}