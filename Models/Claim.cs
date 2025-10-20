namespace ClaimIT.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount => HoursWorked * HourlyRate;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public List<string> DocumentNames { get; set; } = new List<string>();
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
    }

    public class ClaimDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int ClaimId { get; set; }
    }
}