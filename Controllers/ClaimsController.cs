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
            if (ModelState.IsValid)
            {
                claim.Id = _context.Claims.Count + 1;
                claim.SubmittedDate = DateTime.Now;
                claim.Status = "Pending"; // Ensure new claims start as pending

                // Handle file uploads
                if (documents != null && documents.Count > 0)
                {
                    claim.DocumentNames = new List<string>();
                    foreach (var document in documents)
                    {
                        if (document.Length > 0)
                        {
                            // Simple file handling - in real app, save to disk/database
                            claim.DocumentNames.Add(document.FileName);

                            var fileDoc = new ClaimDocument
                            {
                                Id = _context.Documents.Count + 1,
                                FileName = document.FileName,
                                ContentType = document.ContentType,
                                FileSize = document.Length,
                                ClaimId = claim.Id
                            };
                            _context.Documents.Add(fileDoc);
                        }
                    }
                }

                _context.Claims.Add(claim);
                return RedirectToAction(nameof(Index));
            }
            return View(claim);
        }

        // New action for approval workflow
        public IActionResult Approve(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Approved";
                claim.ApprovedDate = DateTime.Now;
                claim.ApprovedBy = "System"; // In real app, get from logged-in user
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Verify(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Verified";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Reject(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Rejected";
            }
            return RedirectToAction(nameof(Index));
        }

        // Coordinator/Manager view
        public IActionResult ApprovalQueue()
        {
            var pendingClaims = _context.Claims.Where(c => c.Status == "Pending" || c.Status == "Verified").ToList();
            return View(pendingClaims);
        }

        // Claim details view
        public IActionResult Details(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                return NotFound();
            }
            return View(claim);
        }
    }
}