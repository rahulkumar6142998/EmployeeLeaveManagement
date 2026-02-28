using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Helpers;
using EmployeeLeaveManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to appropriate dashboard
            if (SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                if (SessionHelper.IsAdmin(HttpContext.Session))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("Dashboard", "Employee");
                }
            }

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password, bool RememberMe = false)
        {
            // Check if email and password provided
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View();
            }

            try
            {
                // Find user by email and password
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email && u.Password == Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View();
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact administrator.");
                    return View();
                }

                // Set session
                SessionHelper.SetUserSession(
                    HttpContext.Session,
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.Role
                );

                // Redirect based on role
                if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";
                    return RedirectToAction("Dashboard", "Employee");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during login: " + ex.Message);
                return View();
            }
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            SessionHelper.ClearSession(HttpContext.Session);
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}