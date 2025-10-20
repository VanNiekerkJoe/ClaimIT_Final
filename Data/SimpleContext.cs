using System.Collections.Generic;
using ClaimIT.Models;

namespace ClaimIT.Data
{
    public class SimpleContext
    {
        public List<Claim> Claims { get; set; } = new List<Claim>
        {
            new Claim {
                Id = 1,
                LecturerName = "Dr. Sarah Smith",
                HoursWorked = 40,
                HourlyRate = 150,
                Status = "Pending",
                DocumentNames = new List<string> { "timesheet.pdf", "contract.docx" },
                SubmittedDate = DateTime.Now.AddDays(-2)
            },
            new Claim {
                Id = 2,
                LecturerName = "Prof. James Johnson",
                HoursWorked = 35,
                HourlyRate = 180,
                Status = "Verified",
                DocumentNames = new List<string> { "research_hours.xlsx" },
                SubmittedDate = DateTime.Now.AddDays(-1)
            },
            new Claim {
                Id = 3,
                LecturerName = "Dr. Maria Brown",
                HoursWorked = 45,
                HourlyRate = 160,
                Status = "Approved",
                DocumentNames = new List<string> { "curriculum_plan.pdf", "meeting_notes.docx" },
                SubmittedDate = DateTime.Now.AddDays(-3),
                ApprovedDate = DateTime.Now.AddDays(-1),
                ApprovedBy = "Academic Coordinator"
            }
        };

        public List<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
    }
}