// Models/Claim.cs
using System.ComponentModel.DataAnnotations;

namespace ClaimIT.Models
{
    public class Claim
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lecturer name is required")]
        public string LecturerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours must be between 1 and 200")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(20, 500, ErrorMessage = "Hourly rate must be between R20 and R500")]
        public decimal HourlyRate { get; set; }

        public decimal TotalAmount => HoursWorked * HourlyRate;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public List<string> DocumentNames { get; set; } = new List<string>();
        public List<string> DocumentPaths { get; set; } = new List<string>(); // New property for file paths
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
    }

    public class ClaimDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty; // Unique stored filename
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int ClaimId { get; set; }
    }
}