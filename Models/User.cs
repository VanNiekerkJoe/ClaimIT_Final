// Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace ClaimIT.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty; // In production, use hashed passwords

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Lecturer, Coordinator, Manager, HR

        public decimal HourlyRate { get; set; }
        public string Department { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }

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

    public class ReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Now;
        public string ReportType { get; set; } = "Monthly";
        public List<Claim> Claims { get; set; } = new List<Claim>();
        public decimal TotalAmount => Claims.Sum(c => c.TotalAmount);
        public int TotalClaims => Claims.Count;
    }
}