// Data/EnhancedContext.cs
using ClaimIT.Models;

namespace ClaimIT.Data
{
    public class EnhancedContext
    {
        private static List<User> _users = new List<User>
        {
            new User {
                Id = 1,
                Email = "lecturer@university.com",
                Password = "lecturer123",
                FullName = "Dr. Sarah Smith",
                Role = "Lecturer",
                HourlyRate = 150,
                Department = "Computer Science"
            },
            new User {
                Id = 2,
                Email = "coordinator@university.com",
                Password = "coordinator123",
                FullName = "Prof. James Johnson",
                Role = "Coordinator",
                Department = "Academic Affairs"
            },
            new User {
                Id = 3,
                Email = "manager@university.com",
                Password = "manager123",
                FullName = "Dr. Maria Brown",
                Role = "Manager",
                Department = "Academic Management"
            },
            new User {
                Id = 4,
                Email = "hr@university.com",
                Password = "hr123",
                FullName = "HR Administrator",
                Role = "HR",
                Department = "Human Resources"
            }
        };

        private static List<Claim> _claims = new List<Claim>
        {
            new Claim {
                Id = 1,
                LecturerName = "Dr. Sarah Smith",
                LecturerEmail = "lecturer@university.com",
                HoursWorked = 40,
                HourlyRate = 150,
                Status = "Pending",
                DocumentNames = new List<string> { "timesheet.pdf", "contract.docx" },
                DocumentPaths = new List<string> { "file1.pdf", "file2.docx" },
                SubmittedDate = DateTime.Now.AddDays(-2)
            },
            new Claim {
                Id = 2,
                LecturerName = "Dr. Sarah Smith",
                LecturerEmail = "lecturer@university.com",
                HoursWorked = 35,
                HourlyRate = 150,
                Status = "Verified",
                DocumentNames = new List<string> { "research_hours.xlsx" },
                DocumentPaths = new List<string> { "file3.xlsx" },
                SubmittedDate = DateTime.Now.AddDays(-1)
            },
            new Claim {
                Id = 3,
                LecturerName = "Dr. Sarah Smith",
                LecturerEmail = "lecturer@university.com",
                HoursWorked = 45,
                HourlyRate = 150,
                Status = "Approved",
                DocumentNames = new List<string> { "curriculum_plan.pdf", "meeting_notes.docx" },
                DocumentPaths = new List<string> { "file4.pdf", "file5.docx" },
                SubmittedDate = DateTime.Now.AddDays(-3),
                ApprovedDate = DateTime.Now.AddDays(-1),
                ApprovedBy = "Prof. James Johnson"
            }
        };

        private static List<ClaimDocument> _documents = new List<ClaimDocument>();
        private static int _nextClaimId = 4;
        private static int _nextUserId = 5;

        public List<User> Users => _users;
        public List<Claim> Claims => _claims;
        public List<ClaimDocument> Documents => _documents;
        public int NextClaimId => _nextClaimId;
        public int NextUserId => _nextUserId;

        public User? AuthenticateUser(string email, string password)
        {
            return _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password &&
                u.IsActive);
        }

        public User? GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public void AddUser(User user)
        {
            user.Id = _nextUserId;
            _users.Add(user);
            _nextUserId++;
        }

        public void UpdateUser(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;
                existingUser.HourlyRate = user.HourlyRate;
                existingUser.Department = user.Department;
                existingUser.IsActive = user.IsActive;
            }
        }

        public void AddClaim(Claim claim)
        {
            claim.Id = _nextClaimId;
            _claims.Add(claim);
            _nextClaimId++;
        }

        public void UpdateClaimStatus(int claimId, string status, string approvedBy = null)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == claimId);
            if (claim != null)
            {
                claim.Status = status;
                if (status == "Approved")
                {
                    claim.ApprovedDate = DateTime.Now;
                    claim.ApprovedBy = approvedBy ?? "System";
                }
            }
        }

        public void AddDocument(ClaimDocument document)
        {
            _documents.Add(document);
        }

        public List<Claim> GetClaimsByLecturer(string lecturerEmail)
        {
            return _claims.Where(c => c.LecturerEmail == lecturerEmail).ToList();
        }

        public List<Claim> GetClaimsForApproval()
        {
            return _claims.Where(c => c.Status == "Pending" || c.Status == "Verified").ToList();
        }

        public List<Claim> GetClaimsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _claims.Where(c => c.SubmittedDate >= startDate && c.SubmittedDate <= endDate).ToList();
        }
    }
}