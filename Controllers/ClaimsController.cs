// Controllers/ClaimsController.cs
using Microsoft.AspNetCore.Mvc;
using ClaimIT.Data;
using ClaimIT.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClaimIT.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ClaimITContext _context;
        private readonly IWebHostEnvironment _environment;

        public ClaimsController(ClaimITContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Helper methods
        private string UserEmail => HttpContext.Session.GetString("UserEmail") ?? "";
        private string UserName => HttpContext.Session.GetString("UserName") ?? "";
        private string UserRole => HttpContext.Session.GetString("UserRole") ?? "";

        private bool IsAuthenticated() => !string.IsNullOrEmpty(UserEmail);
        private bool HasRole(string role) => UserRole == role;

        public IActionResult Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var claims = UserRole switch
            {
                "Lecturer" => _context.Claims.Where(c => c.LecturerEmail == UserEmail),
                "Coordinator" or "Manager" => _context.Claims,
                "HR" => _context.Claims,
                _ => _context.Claims.Take(0)
            };

            var claimList = claims.OrderByDescending(c => c.SubmittedDate).ToList();

            ViewBag.TotalClaims = claimList.Count;
            ViewBag.PendingClaims = claimList.Count(c => c.Status == "Pending");
            ViewBag.VerifiedClaims = claimList.Count(c => c.Status == "Verified");
            ViewBag.ApprovedClaims = claimList.Count(c => c.Status == "Approved");
            ViewBag.RejectedClaims = claimList.Count(c => c.Status == "Rejected");
            ViewBag.TotalAmount = claimList.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);
            ViewBag.UserRole = UserRole;
            ViewBag.UserName = UserName;

            return View(claimList);
        }

        public IActionResult Create()
        {
            if (!IsAuthenticated() || !HasRole("Lecturer"))
                return RedirectToAction("AccessDenied", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.Email == UserEmail);
            if (user != null)
            {
                ViewBag.DefaultHourlyRate = user.HourlyRate;
                ViewBag.LecturerName = user.FullName;
                ViewBag.LecturerEmail = user.Email;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile[] documents)
        {
            if (!IsAuthenticated() || !HasRole("Lecturer"))
                return RedirectToAction("AccessDenied", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.DefaultHourlyRate = _context.Users.FirstOrDefault(u => u.Email == UserEmail)?.HourlyRate;
                return View(claim);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == UserEmail);
            claim.LecturerName = user?.FullName ?? "Unknown";
            claim.LecturerEmail = UserEmail;
            claim.SubmittedDate = DateTime.Now;
            claim.Status = "Pending";

            // File upload handling
            var names = new List<string>();
            var paths = new List<string>();

            if (documents != null && documents.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                foreach (var file in documents)
                {
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", $"{file.FileName} exceeds 5MB limit.");
                        return View(claim);
                    }

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".jpeg", ".png" }.Contains(ext))
                    {
                        ModelState.AddModelError("", $"Invalid file type: {file.FileName}");
                        return View(claim);
                    }

                    var uniqueName = Guid.NewGuid() + ext;
                    var filePath = Path.Combine(uploadsFolder, uniqueName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await file.CopyToAsync(stream);

                    names.Add(file.FileName);
                    paths.Add("/uploads/" + uniqueName);
                }
            }

            claim.DocumentNamesJson = JsonSerializer.Serialize(names);
            claim.DocumentPathsJson = JsonSerializer.Serialize(paths);

            // Save to database
            _context.Claims.Add(claim);
            _context.ClaimAudits.Add(new ClaimAudit
            {
                ClaimId = claim.Id,
                Action = "Submitted",
                PerformedBy = UserName,
                PerformedByRole = "Lecturer",
                Timestamp = DateTime.Now
            });

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Claim #{claim.Id} submitted successfully!";
            return RedirectToAction("Index");
        }

        // Verify / Approve / Reject
        public IActionResult Verify(int id) => UpdateStatus(id, "Verified", "verified");
        public IActionResult Approve(int id) => UpdateStatus(id, "Approved", "approved");
        public IActionResult Reject(int id) => UpdateStatus(id, "Rejected", "rejected");

        private IActionResult UpdateStatus(int id, string status, string action)
        {
            if (!IsAuthenticated() || (!HasRole("Coordinator") && !HasRole("Manager")))
                return RedirectToAction("AccessDenied", "Auth");

            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("Index");
            }

            claim.Status = status;
            if (status == "Approved") claim.ApprovedDate = DateTime.Now;
            if (status == "Verified") claim.VerifiedDate = DateTime.Now;

            _context.ClaimAudits.Add(new ClaimAudit
            {
                ClaimId = id,
                Action = status,
                PerformedBy = UserName,
                PerformedByRole = UserRole,
                Timestamp = DateTime.Now
            });

            _context.SaveChanges();
            TempData["SuccessMessage"] = $"Claim #{id} {action} successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null) return NotFound();

            if (UserRole == "Lecturer" && claim.LecturerEmail != UserEmail)
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.UserRole = UserRole;
            ViewBag.UserName = UserName;
            ViewBag.CanVerify = (UserRole == "Coordinator" || UserRole == "Manager") && claim.Status == "Pending";
            ViewBag.CanApprove = (UserRole == "Manager") && claim.Status == "Verified";
            ViewBag.CanReject = (UserRole == "Coordinator" || UserRole == "Manager") && claim.Status != "Approved";

            return View(claim);
        }

        public IActionResult ViewDocument(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return NotFound();
            var path = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            if (!System.IO.File.Exists(path)) return NotFound();

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            return PhysicalFile(path, contentType);
        }

        public IActionResult DownloadDocument(string fileName, string originalName = null)
        {
            if (string.IsNullOrEmpty(fileName)) return NotFound();
            var path = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            if (!System.IO.File.Exists(path)) return NotFound();

            return PhysicalFile(path, "application/octet-stream", originalName ?? fileName);
        }
    }
}