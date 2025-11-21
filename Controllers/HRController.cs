// Controllers/HRController.cs
using Microsoft.AspNetCore.Mvc;
using ClaimIT.Data;
using ClaimIT.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimIT.Controllers
{
    public class HRController : Controller
    {
        private readonly ClaimITContext _context;  // Changed from EnhancedContext

        // This is the correct way – dependency injection
        public HRController(ClaimITContext context)
        {
            _context = context;
        }

        private bool IsAuthenticatedHR()
        {
            return HttpContext.Session.GetString("UserRole") == "HR";
        }

        public IActionResult Index()
        {
            if (!IsAuthenticatedHR())
                return RedirectToAction("AccessDenied", "Auth");

            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult CreateUser()
        {
            if (!IsAuthenticatedHR())
                return RedirectToAction("AccessDenied", "Auth");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User user)
        {
            if (!IsAuthenticatedHR())
                return RedirectToAction("AccessDenied", "Auth");

            if (!ModelState.IsValid)
                return View(user);

            // Check if email already exists
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email address already exists.");
                return View(user);
            }

            // Set default values
            user.PasswordHash = user.PasswordHash; // You can hash later if you want
            user.CreatedDate = DateTime.Now;
            user.IsActive = true;

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"User {user.FullName} created successfully!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult EditUser(int id)
        {
            if (!IsAuthenticatedHR())
                return RedirectToAction("AccessDenied", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User user)
        {
            if (!IsAuthenticatedHR())
                return RedirectToAction("AccessDenied", "Auth");

            if (!ModelState.IsValid)
                return View(user);

            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"User {user.FullName} updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Reports()
        {
            if (!IsAuthenticatedHR())
                return RedirectToAction("AccessDenied", "Auth");

            var model = new ReportViewModel
            {
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now,
                Claims = _context.Claims.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult GenerateReport(ReportViewModel model)
        {
            if (!IsAuthenticatedHR())
                return RedirectToAction("AccessDenied", "Auth");

            model.Claims = _context.Claims
                .Where(c => c.SubmittedDate >= model.StartDate && c.SubmittedDate <= model.EndDate)
                .ToList();

            if (model.ReportType == "Monthly")
            {
                model.Claims = model.Claims
                    .Where(c => c.Status == "Approved")
                    .OrderBy(c => c.SubmittedDate)
                    .ToList();
            }

            TempData["SuccessMessage"] = $"Report generated for {model.StartDate:yyyy-MM-dd} to {model.EndDate:yyyy-MM-dd}. " +
                                        $"Found {model.TotalClaims} claims totaling R {model.TotalAmount:N2}";
            return View("Reports", model);
        }

        public IActionResult GenerateInvoice(int claimId)
        {
            if (!IsAuthenticatedHR())
                return RedirectToAction("AccessDenied", "Auth");

            var claim = _context.Claims.FirstOrDefault(c => c.Id == claimId);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("Reports");
            }

            ViewBag.InvoiceNumber = $"INV-{claim.Id:0000}-{DateTime.Now:yyyyMMdd}";
            ViewBag.InvoiceDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.DueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
            return View(claim);
        }
    }
}