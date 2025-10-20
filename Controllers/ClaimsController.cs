using Microsoft.AspNetCore.Mvc;
using ClaimIT.Data;
using ClaimIT.Models;

namespace ClaimIT.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly SimpleContext _context;
        private readonly IWebHostEnvironment _environment;

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
        public IActionResult Create(Claim claim, List<IFormFile> documents)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validate file sizes and types
                    if (documents != null && documents.Count > 0)
                    {
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
                        }

                        claim.DocumentNames = documents.Select(d => d.FileName).ToList();
                    }

                    claim.Id = _context.Claims.Count + 1;
                    claim.SubmittedDate = DateTime.Now;
                    claim.Status = "Pending";

                    _context.Claims.Add(claim);

                    TempData["SuccessMessage"] = "Claim submitted successfully!";
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
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Approved";
                claim.ApprovedDate = DateTime.Now;
                claim.ApprovedBy = "System";
                TempData["SuccessMessage"] = $"Claim #{id} approved successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Verify(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Verified";
                TempData["SuccessMessage"] = $"Claim #{id} verified successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Reject(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                TempData["SuccessMessage"] = $"Claim #{id} rejected.";
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
    }
}