using Microsoft.AspNetCore.Mvc;
using ClaimIT.Data;
using ClaimIT.Models;

namespace ClaimIT.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly SimpleContext _context;
        private readonly IWebHostEnvironment _environment;

        // Updated constructor - simpler dependency injection
        public ClaimsController(IWebHostEnvironment environment)
        {
            _context = new SimpleContext();
            _environment = environment;
        }

        public IActionResult Index()
        {
            var claims = _context.Claims;

            // Dashboard statistics
            ViewBag.TotalClaims = claims.Count;
            ViewBag.PendingClaims = claims.Count(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            ViewBag.VerifiedClaims = claims.Count(c => c.Status == "Verified");

            return View(claims);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, List<IFormFile> documents)
        {
            try
            {
                if (ModelState.IsValid)
                {
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

                return View(claim);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while submitting the claim. Please try again.");
                return View(claim);
            }
        }

        public IActionResult Approve(int id)
        {
            try
            {
                _context.UpdateClaimStatus(id, "Approved", "Coordinator");
                TempData["SuccessMessage"] = $"Claim #{id} approved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving claim #{id}. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Verify(int id)
        {
            try
            {
                _context.UpdateClaimStatus(id, "Verified");
                TempData["SuccessMessage"] = $"Claim #{id} verified successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error verifying claim #{id}. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Reject(int id)
        {
            try
            {
                _context.UpdateClaimStatus(id, "Rejected");
                TempData["SuccessMessage"] = $"Claim #{id} rejected.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting claim #{id}. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ApprovalQueue()
        {
            var pendingClaims = _context.Claims.Where(c => c.Status == "Pending" || c.Status == "Verified").ToList();
            return View(pendingClaims);
        }

        public IActionResult Details(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(claim);
        }

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
    }
}