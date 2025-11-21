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

        // Helper Properties (Session-based auth)
        private string UserEmail => HttpContext.Session.GetString("UserEmail") ?? "";
        private string UserName => HttpContext.Session.GetString("UserName") ?? "";
        private string UserRole => HttpContext.Session.GetString("UserRole") ?? "";
        private bool IsAuthenticated() => !string.IsNullOrEmpty(UserEmail);
        private bool HasRole(string role) => UserRole == role;
        private bool IsCoordinatorOrManager => HasRole("Coordinator") || HasRole("Manager");

        // ==================================================================
        // 1. MAIN CLAIMS LIST
        // ==================================================================
        public IActionResult Index()
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var claims = UserRole switch
            {
                "Lecturer" => _context.Claims.Where(c => c.LecturerEmail == UserEmail),
                _ => _context.Claims
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

        // ==================================================================
        // 2. APPROVAL QUEUE – Fixed: No .Include(c => c.User), No 'Title'
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> ApprovalQueue(string search, string statusFilter, int page = 1)
        {
            if (!IsAuthenticated() || !IsCoordinatorOrManager)
                return RedirectToAction("AccessDenied", "Auth");

            const int pageSize = 15;

            var query = _context.Claims
                .Where(c => c.Status == "Pending" || c.Status == "Verified")
                .AsQueryable();

            // Search – using only real fields
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c =>
                    c.LecturerName.ToLower().Contains(search) ||
                    c.LecturerEmail.ToLower().Contains(search) ||
                    c.Id.ToString().Contains(search));
            }

            // Status Filter
            if (!string.IsNullOrEmpty(statusFilter) && (statusFilter == "Pending" || statusFilter == "Verified"))
            {
                query = query.Where(c => c.Status == statusFilter);
            }

            var total = await query.CountAsync();

            var claims = await query
                .OrderByDescending(c => c.SubmittedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.TotalPending = await _context.Claims.CountAsync(c => c.Status == "Pending" || c.Status == "Verified");

            return View(claims);
        }

        // ==================================================================
        // 3. BULK APPROVE – Works perfectly
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkApprove(List<int> selectedClaims)
        {
            if (!IsAuthenticated() || !IsCoordinatorOrManager)
                return RedirectToAction("AccessDenied", "Auth");

            if (selectedClaims == null || !selectedClaims.Any())
            {
                TempData["Warning"] = "No claims were selected.";
                return RedirectToAction("ApprovalQueue");
            }

            var claims = await _context.Claims
                .Where(c => selectedClaims.Contains(c.Id) && (c.Status == "Pending" || c.Status == "Verified"))
                .ToListAsync();

            foreach (var claim in claims)
            {
                claim.Status = "Approved";
                claim.ApprovedDate = DateTime.Now;

                _context.ClaimAudits.Add(new ClaimAudit
                {
                    ClaimId = claim.Id,
                    Action = "Approved (Bulk)",
                    PerformedBy = UserName,
                    PerformedByRole = UserRole,
                    Timestamp = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{claims.Count} claim(s) approved successfully!";
            return RedirectToAction("ApprovalQueue");
        }

        // ==================================================================
        // 4. CREATE CLAIM (Lecturer Only)
        // ==================================================================
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

            // File Upload Handling
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
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    names.Add(file.FileName);
                    paths.Add("/uploads/" + uniqueName);
                }
            }

            claim.DocumentNamesJson = JsonSerializer.Serialize(names);
            claim.DocumentPathsJson = JsonSerializer.Serialize(paths);

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

        // ==================================================================
        // 5. STATUS ACTIONS
        // ==================================================================
        public IActionResult Verify(int id) => UpdateStatus(id, "Verified", "verified");
        public IActionResult Approve(int id) => UpdateStatus(id, "Approved", "approved");
        public IActionResult Reject(int id) => UpdateStatus(id, "Rejected", "rejected");

        private IActionResult UpdateStatus(int id, string status, string action)
        {
            if (!IsAuthenticated() || !IsCoordinatorOrManager)
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
            return RedirectToAction("ApprovalQueue");
        }

        // ==================================================================
        // 6. DETAILS & DOCUMENTS
        // ==================================================================
        public IActionResult Details(int id)
        {
            if (!IsAuthenticated()) return RedirectToAction("Login", "Auth");

            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null) return NotFound();

            if (UserRole == "Lecturer" && claim.LecturerEmail != UserEmail)
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.UserRole = UserRole;
            ViewBag.CanVerify = IsCoordinatorOrManager && claim.Status == "Pending";
            ViewBag.CanApprove = IsCoordinatorOrManager && claim.Status == "Verified";
            ViewBag.CanReject = IsCoordinatorOrManager && claim.Status != "Approved" && claim.Status != "Rejected";

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