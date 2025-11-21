// Models/ClaimAudit.cs
using System.ComponentModel.DataAnnotations;

namespace ClaimIT.Models
{
    public class ClaimAudit
    {
        [Key]
        public int Id { get; set; }

        public int ClaimId { get; set; }

        public string Action { get; set; } = string.Empty; // Submitted, Verified, Approved, Rejected

        public string PerformedBy { get; set; } = string.Empty;

        public string PerformedByRole { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string? Notes { get; set; }
    }
}