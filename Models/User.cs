// Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace ClaimIT.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // We're keeping plain text password only for demo/final-year project
        // In real apps: use PasswordHash with ASP.NET Identity
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Lecturer"; // Lecturer, Coordinator, Manager, HR

        public decimal HourlyRate { get; set; } = 280.00m;

        public string Department { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }

    // Login ViewModel (used in Auth/Login)
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    // Optional: Report ViewModel for Reports (you can use later)
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Today;
        public string ReportType { get; set; } = "Monthly";
        public List<Claim> Claims { get; set; } = new();
        public decimal TotalAmount => Claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);
        public int TotalClaims => Claims.Count;
    }
}