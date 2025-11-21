using Microsoft.AspNetCore.Mvc;
using ClaimIT.Data;
using ClaimIT.Models;
using Microsoft.AspNetCore.Hosting;

namespace ClaimIT.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly EnhancedContext _context;
        private readonly IWebHostEnvironment _environment;

        public ClaimsController(IWebHostEnvironment environment)
        {
            _context = new EnhancedContext();
            _environment = environment;
        }

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("UserEmail") != null;
        }

        private bool HasRole(string role)
        {
            return HttpContext.Session.GetString("UserRole") == role;
        }

        private string GetCurrentUserEmail()
        {
            return HttpContext.Session.GetString("UserEmail") ?? string.Empty;
        }

        private string GetCurrentUserName()
        {
            return HttpContext.Session.GetString("UserName") ?? string.Empty;
        }

        private string GetCurrentUserRole()
        {
            return HttpContext.Session.GetString("UserRole") ?? string.Empty;
        }

        // GET: Claims/Index
        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userRole = GetCurrentUserRole();
            var userEmail = GetCurrentUserEmail();
            List<Claim> claims;

            switch (userRole)
            {
                case "Lecturer":
                    claims = _context.GetClaimsByLecturer(userEmail);
                    break;
                case "Coordinator":
                case "Manager":
                    claims = _context.GetClaimsForApproval();
                    break;
                case "HR":
                    claims = _context.Claims;
                    break;
                default:
                    claims = new List<Claim>();
                    break;
            }

            // Dashboard statistics
            ViewBag.TotalClaims = claims.Count;
            ViewBag.PendingClaims = claims.Count(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            ViewBag.VerifiedClaims = claims.Count(c => c.Status == "Verified");
            ViewBag.RejectedClaims = claims.Count(c => c.Status == "Rejected");
            ViewBag.UserRole = userRole;
            ViewBag.UserName = GetCurrentUserName();
            ViewBag.UserEmail = userEmail;

            // Calculate total amounts
            ViewBag.TotalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);
            ViewBag.PendingAmount = claims.Where(c => c.Status == "Pending").Sum(c => c.TotalAmount);
            ViewBag.VerifiedAmount = claims.Where(c => c.Status == "Verified").Sum(c => c.TotalAmount);

            return View(claims);
        }

        // GET: Claims/Create
        public IActionResult Create()
        {
            if (!IsAuthenticated() || !HasRole("Lecturer"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var user = _context.GetUserByEmail(GetCurrentUserEmail());
            if (user != null)
            {
                ViewBag.DefaultHourlyRate = user.HourlyRate;
                ViewBag.LecturerName = user.FullName;
                ViewBag.LecturerEmail = user.Email;
            }

            return View();
        }

        // POST: Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, List<IFormFile> documents)
        {
            if (!IsAuthenticated() || !HasRole("Lecturer"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var user = _context.GetUserByEmail(GetCurrentUserEmail());
                    if (user != null)
                    {
                        // Auto-populate lecturer information from logged-in user
                        claim.LecturerName = user.FullName;
                        claim.LecturerEmail = user.Email;

                        // Use lecturer's hourly rate if not specified or invalid
                        if (claim.HourlyRate == 0 || claim.HourlyRate < 20 || claim.HourlyRate > 500)
                        {
                            claim.HourlyRate = user.HourlyRate;
                        }
                    }

                    claim.DocumentNames = new List<string>();
                    claim.DocumentPaths = new List<string>();

                    // Handle file uploads
                    if (documents != null && documents.Count > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        foreach (var document in documents)
                        {
                            if (document.Length > 5 * 1024 * 1024) // 5MB limit
                            {
                                ModelState.AddModelError("", $"File '{document.FileName}' is too large. Maximum size is 5MB.");
                                return View(claim);
                            }

                            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png", ".jpeg" };
                            var fileExtension = Path.GetExtension(document.FileName).ToLowerInvariant();
                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("", $"File '{document.FileName}' has invalid type. Allowed types: PDF, Word, Excel, Images.");
                                return View(claim);
                            }

                            // Generate unique filename
                            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await document.CopyToAsync(fileStream);
                            }

                            claim.DocumentNames.Add(document.FileName);
                            claim.DocumentPaths.Add(uniqueFileName);

                            // Add to documents list
                            var fileDoc = new ClaimDocument
                            {
                                Id = _context.Documents.Count + 1,
                                FileName = document.FileName,
                                StoredFileName = uniqueFileName,
                                ContentType = document.ContentType,
                                FileSize = document.Length,
                                ClaimId = _context.NextClaimId
                            };
                            _context.Documents.Add(fileDoc);
                        }
                    }

                    claim.SubmittedDate = DateTime.Now;
                    claim.Status = "Pending";
                    _context.AddClaim(claim);

                    TempData["SuccessMessage"] = $"Claim #{claim.Id} submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // If we got this far, something failed; redisplay form
                var currentUser = _context.GetUserByEmail(GetCurrentUserEmail());
                if (currentUser != null)
                {
                    ViewBag.DefaultHourlyRate = currentUser.HourlyRate;
                    ViewBag.LecturerName = currentUser.FullName;
                    ViewBag.LecturerEmail = currentUser.Email;
                }
                return View(claim);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application)
                Console.WriteLine($"Error submitting claim: {ex.Message}");

                ModelState.AddModelError("", "An error occurred while submitting the claim. Please try again.");

                // Re-populate viewbag data
                var currentUser = _context.GetUserByEmail(GetCurrentUserEmail());
                if (currentUser != null)
                {
                    ViewBag.DefaultHourlyRate = currentUser.HourlyRate;
                    ViewBag.LecturerName = currentUser.FullName;
                    ViewBag.LecturerEmail = currentUser.Email;
                }
                return View(claim);
            }
        }

        // GET: Claims/Approve/{id}
        public IActionResult Approve(int id)
        {
            if (!IsAuthenticated() || (!HasRole("Coordinator") && !HasRole("Manager")))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                _context.UpdateClaimStatus(id, "Approved", GetCurrentUserName());
                TempData["SuccessMessage"] = $"Claim #{id} approved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving claim #{id}. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Claims/Verify/{id}
        public IActionResult Verify(int id)
        {
            if (!IsAuthenticated() || (!HasRole("Coordinator") && !HasRole("Manager")))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                _context.UpdateClaimStatus(id, "Verified", GetCurrentUserName());
                TempData["SuccessMessage"] = $"Claim #{id} verified successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error verifying claim #{id}. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Claims/Reject/{id}
        public IActionResult Reject(int id)
        {
            if (!IsAuthenticated() || (!HasRole("Coordinator") && !HasRole("Manager")))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                _context.UpdateClaimStatus(id, "Rejected", GetCurrentUserName());
                TempData["SuccessMessage"] = $"Claim #{id} rejected.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting claim #{id}. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Claims/ApprovalQueue
        public IActionResult ApprovalQueue()
        {
            if (!IsAuthenticated() || (!HasRole("Coordinator") && !HasRole("Manager")))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var pendingClaims = _context.GetClaimsForApproval();
            ViewBag.UserRole = GetCurrentUserRole();
            ViewBag.UserName = GetCurrentUserName();
            ViewBag.TotalPending = pendingClaims.Count(c => c.Status == "Pending");
            ViewBag.TotalVerified = pendingClaims.Count(c => c.Status == "Verified");

            return View(pendingClaims);
        }

        // GET: Claims/Details/{id}
        public IActionResult Details(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check access rights
            var userRole = GetCurrentUserRole();
            var userEmail = GetCurrentUserEmail();

            if (userRole == "Lecturer" && claim.LecturerEmail != userEmail)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            ViewBag.UserRole = userRole;
            ViewBag.UserName = GetCurrentUserName();
            ViewBag.CanApprove = (userRole == "Coordinator" || userRole == "Manager") && claim.Status == "Verified";
            ViewBag.CanVerify = (userRole == "Coordinator" || userRole == "Manager") && claim.Status == "Pending";
            ViewBag.CanReject = (userRole == "Coordinator" || userRole == "Manager") && (claim.Status == "Pending" || claim.Status == "Verified");

            return View(claim);
        }

        // GET: Claims/ViewDocument/{fileName}
        public IActionResult ViewDocument(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            return PhysicalFile(filePath, contentType);
        }

        // GET: Claims/DownloadDocument/{fileName}
        public IActionResult DownloadDocument(string fileName, string originalName = null)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var downloadName = originalName ?? fileName;
            return PhysicalFile(filePath, "application/octet-stream", downloadName);
        }

        // POST: Claims/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAuthenticated() || !HasRole("Lecturer"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if the claim belongs to the current user
                if (claim.LecturerEmail != GetCurrentUserEmail())
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }

                // Only allow deletion of pending claims
                if (claim.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "Only pending claims can be deleted.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Claims.Remove(claim);
                TempData["SuccessMessage"] = $"Claim #{id} deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting claim #{id}. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Claims/UpdateHourlyRate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateHourlyRate(decimal newRate)
        {
            if (!IsAuthenticated() || !HasRole("Lecturer"))
            {
                return Json(new { success = false, message = "Access denied" });
            }

            try
            {
                var user = _context.GetUserByEmail(GetCurrentUserEmail());
                if (user != null && newRate >= 20 && newRate <= 500)
                {
                    user.HourlyRate = newRate;
                    TempData["SuccessMessage"] = $"Hourly rate updated to R {newRate} successfully!";
                    return Json(new { success = true, message = "Hourly rate updated successfully" });
                }
                return Json(new { success = false, message = "Invalid rate or user not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating hourly rate" });
            }
        }

        // GET: Claims/MyClaims (Lecturer-specific view)
        public IActionResult MyClaims()
        {
            if (!IsAuthenticated() || !HasRole("Lecturer"))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var userEmail = GetCurrentUserEmail();
            var claims = _context.GetClaimsByLecturer(userEmail);

            ViewBag.TotalClaims = claims.Count;
            ViewBag.PendingClaims = claims.Count(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            ViewBag.VerifiedClaims = claims.Count(c => c.Status == "Verified");
            ViewBag.RejectedClaims = claims.Count(c => c.Status == "Rejected");
            ViewBag.UserRole = GetCurrentUserRole();
            ViewBag.UserName = GetCurrentUserName();

            return View(claims);
        }

        // GET: Claims/Statistics
        public IActionResult Statistics()
        {
            if (!IsAuthenticated() || (!HasRole("HR") && !HasRole("Manager")))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var claims = _context.Claims;
            var users = _context.Users.Where(u => u.Role == "Lecturer" && u.IsActive);

            var statistics = new
            {
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => c.Status == "Pending"),
                ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                VerifiedClaims = claims.Count(c => c.Status == "Verified"),
                RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                TotalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount),
                AverageClaimAmount = claims.Any() ? claims.Average(c => c.TotalAmount) : 0,
                ActiveLecturers = users.Count(),
                ClaimsThisMonth = claims.Count(c => c.SubmittedDate.Month == DateTime.Now.Month && c.SubmittedDate.Year == DateTime.Now.Year)
            };

            ViewBag.Statistics = statistics;
            ViewBag.UserRole = GetCurrentUserRole();
            ViewBag.UserName = GetCurrentUserName();

            return View();
        }

        // Helper method to get claims statistics for the current user
        private Dictionary<string, int> GetUserClaimStatistics()
        {
            var userEmail = GetCurrentUserEmail();
            var userRole = GetCurrentUserRole();
            List<Claim> userClaims;

            if (userRole == "Lecturer")
            {
                userClaims = _context.GetClaimsByLecturer(userEmail);
            }
            else
            {
                userClaims = _context.Claims;
            }

            return new Dictionary<string, int>
            {
                { "Total", userClaims.Count },
                { "Pending", userClaims.Count(c => c.Status == "Pending") },
                { "Verified", userClaims.Count(c => c.Status == "Verified") },
                { "Approved", userClaims.Count(c => c.Status == "Approved") },
                { "Rejected", userClaims.Count(c => c.Status == "Rejected") }
            };
        }

        // Helper method to check if user can modify claim
        private bool CanModifyClaim(Claim claim)
        {
            var userRole = GetCurrentUserRole();
            var userEmail = GetCurrentUserEmail();

            if (userRole == "Lecturer")
            {
                return claim.LecturerEmail == userEmail && claim.Status == "Pending";
            }

            return userRole == "Coordinator" || userRole == "Manager" || userRole == "HR";
        }
    }
}