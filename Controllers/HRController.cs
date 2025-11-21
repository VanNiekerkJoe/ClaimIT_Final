// Controllers/HRController.cs
using Microsoft.AspNetCore.Mvc;
using ClaimIT.Data;
using ClaimIT.Models;

namespace ClaimIT.Controllers
{
    public class HRController : Controller
    {
        private readonly EnhancedContext _context;

        public HRController()
        {
            _context = new EnhancedContext();
        }

        private bool IsAuthenticatedHR()
        {
            return HttpContext.Session.GetString("UserRole") == "HR";
        }

        public IActionResult Index()
        {
            if (!IsAuthenticatedHR())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var users = _context.Users;
            return View(users);
        }

        public IActionResult CreateUser()
        {
            if (!IsAuthenticatedHR())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User user)
        {
            if (!IsAuthenticatedHR())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = _context.GetUserByEmail(user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email address already exists.");
                    return View(user);
                }

                _context.AddUser(user);
                TempData["SuccessMessage"] = $"User {user.FullName} created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        public IActionResult EditUser(int id)
        {
            if (!IsAuthenticatedHR())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

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
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            if (ModelState.IsValid)
            {
                _context.UpdateUser(user);
                TempData["SuccessMessage"] = $"User {user.FullName} updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        public IActionResult Reports()
        {
            if (!IsAuthenticatedHR())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var model = new ReportViewModel
            {
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now,
                Claims = _context.Claims
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult GenerateReport(ReportViewModel model)
        {
            if (!IsAuthenticatedHR())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            model.Claims = _context.GetClaimsByDateRange(model.StartDate, model.EndDate);

            if (model.ReportType == "Monthly")
            {
                model.Claims = model.Claims
                    .Where(c => c.Status == "Approved")
                    .OrderBy(c => c.SubmittedDate)
                    .ToList();
            }

            TempData["SuccessMessage"] = $"Report generated for {model.StartDate:yyyy-MM-dd} to {model.EndDate:yyyy-MM-dd}. Found {model.TotalClaims} claims totaling R {model.TotalAmount:N2}";

            return View("Reports", model);
        }

        public IActionResult GenerateInvoice(int claimId)
        {
            if (!IsAuthenticatedHR())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var claim = _context.Claims.FirstOrDefault(c => c.Id == claimId);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("Reports");
            }

            // In a real application, you would generate a PDF invoice here
            // For this demo, we'll just show a preview
            ViewBag.InvoiceNumber = $"INV-{claim.Id:0000}-{DateTime.Now:yyyyMMdd}";
            ViewBag.InvoiceDate = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.DueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");

            return View(claim);
        }
    }
}