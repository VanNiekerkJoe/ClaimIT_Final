using System.Collections.Generic;
using System.Linq;
using ClaimIT.Models;

namespace ClaimIT.Data
{
    public class SimpleContext
    {
        // Use static to persist data across requests
        private static List<Claim> _claims = new List<Claim>
        {
            new Claim {
                Id = 1,
                LecturerName = "Dr. Sarah Smith",
                HoursWorked = 40,
                HourlyRate = 150,
                Status = "Pending",
                DocumentNames = new List<string> { "timesheet.pdf", "contract.docx" },
                SubmittedDate = System.DateTime.Now.AddDays(-2)
            },
            new Claim {
                Id = 2,
                LecturerName = "Prof. James Johnson",
                HoursWorked = 35,
                HourlyRate = 180,
                Status = "Verified",
                DocumentNames = new List<string> { "research_hours.xlsx" },
                SubmittedDate = System.DateTime.Now.AddDays(-1)
            },
            new Claim {
                Id = 3,
                LecturerName = "Dr. Maria Brown",
                HoursWorked = 45,
                HourlyRate = 160,
                Status = "Approved",
                DocumentNames = new List<string> { "curriculum_plan.pdf", "meeting_notes.docx" },
                SubmittedDate = System.DateTime.Now.AddDays(-3),
                ApprovedDate = System.DateTime.Now.AddDays(-1),
                ApprovedBy = "Academic Coordinator"
            }
        };

        private static List<ClaimDocument> _documents = new List<ClaimDocument>();
        private static int _nextClaimId = 4; // Start from 4 since we have 3 initial claims

        public List<Claim> Claims => _claims;
        public List<ClaimDocument> Documents => _documents;
        public int NextClaimId => _nextClaimId;

        // Method to add new claim
        public void AddClaim(Claim claim)
        {
            claim.Id = _nextClaimId;
            _claims.Add(claim);
            _nextClaimId++;
        }

        // Method to update claim status
        public void UpdateClaimStatus(int claimId, string status, string approvedBy = null)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == claimId);
            if (claim != null)
            {
                claim.Status = status;
                if (status == "Approved")
                {
                    claim.ApprovedDate = System.DateTime.Now;
                    claim.ApprovedBy = approvedBy ?? "System";
                }
            }
        }

        // Add documents
        public void Add(ClaimDocument document)
        {
            _documents.Add(document);
        }
    }
}