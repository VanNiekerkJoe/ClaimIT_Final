// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using ClaimIT.Data;
using ClaimIT.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimIT.Controllers
{
    public class AuthController : Controller
    {
        private readonly ClaimITContext _context;

        // Dependency injection — this is the correct way
        public AuthController(ClaimITContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
                return RedirectToAction("Index", "Claims");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // REAL database check
            var user = _context.Users
                .FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == model.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetInt32("UserId", user.Id);

                TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";
                return RedirectToAction("Index", "Claims");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}