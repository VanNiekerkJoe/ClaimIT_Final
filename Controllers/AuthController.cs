// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using ClaimIT.Data;
using ClaimIT.Models;
using System.Diagnostics;

namespace ClaimIT.Controllers
{
    public class AuthController : Controller
    {
        private readonly EnhancedContext _context;

        public AuthController()
        {
            _context = new EnhancedContext();
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Claims");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.AuthenticateUser(model.Email, model.Password);
                if (user != null)
                {
                    // Set session variables
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetInt32("UserId", user.Id);

                    TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";
                    return RedirectToAction("Index", "Claims");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}