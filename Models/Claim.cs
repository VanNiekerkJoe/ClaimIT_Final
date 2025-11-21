// Models/Claim.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ClaimIT.Models
{
    public class Claim
    {
        public int Id { get; set; }

        [Required]
        public string LecturerName { get; set; } = string.Empty;

        [Required]
        public string LecturerEmail { get; set; } = string.Empty;

        [Required]
        public decimal HoursWorked { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }

        // COMPLETELY remove TotalAmount from the entity for now
        // We'll handle this differently
        [NotMapped]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        public string? Notes { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? VerifiedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }

        // Stored as JSON in DB
        public string? DocumentNamesJson { get; set; }
        public string? DocumentPathsJson { get; set; }

        // Helper properties (not mapped to DB)
        [NotMapped]
        public List<string>? DocumentNames
        {
            get => string.IsNullOrEmpty(DocumentNamesJson)
                ? null
                : JsonSerializer.Deserialize<List<string>>(DocumentNamesJson);
            set => DocumentNamesJson = value == null ? null : JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string>? DocumentPaths
        {
            get => string.IsNullOrEmpty(DocumentPathsJson)
                    ? null
                    : JsonSerializer.Deserialize<List<string>>(DocumentPathsJson);
            set => DocumentPathsJson = value == null ? null : JsonSerializer.Serialize(value);
        }
    }
}