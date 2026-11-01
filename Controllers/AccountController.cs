using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;
using Hostel_hub.Services;
using Hostel_hub.ViewModels;

namespace Hostel_hub.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, IAuthService authService, ILogger<AccountController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Email = model.Email.Trim().ToLowerInvariant();
            model.RollNumber = model.RollNumber.Trim();
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                _logger.LogWarning("Registration failed: Email {Email} is already registered.", model.Email);
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View(model);
            }

            bool rollNumberExists = await _context.Students.AnyAsync(s => s.RollNumber == model.RollNumber);
            if (rollNumberExists)
            {
                _logger.LogWarning("Registration failed: Roll number {RollNumber} is already registered.", model.RollNumber);
                ModelState.AddModelError(nameof(model.RollNumber), "This roll number is already registered.");
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = _authService.HashPassword(model.Password),
                Role = UserRole.Student
            };
            _context.Users.Add(user);

            var student = new Student
            {
                User = user,
                FullName = model.FullName,
                RollNumber = model.RollNumber
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student registered successfully with UserId {UserId}, RollNumber {RollNumber}.", user.UserId, student.RollNumber);

            await _authService.SignInAsync(HttpContext, user.UserId, user.Email, user.Role.ToString());

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            model.Email = model.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email);

            if (user == null || !_authService.VerifyPassword(model.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email {Email}.", model.Email);
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            if (user.Role == UserRole.MaintenanceStaff)
            {
                var staff = await _context.MaintenanceStaff.FirstOrDefaultAsync(s => s.UserId == user.UserId);
                if (staff == null || !staff.IsActive)
                {
                    _logger.LogWarning("Login attempt for deactivated maintenance staff UserId {UserId}, Email {Email}.", user.UserId, user.Email);
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact the administrator.");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View(model);
                }
            }

            _logger.LogInformation("User {UserId} logged in successfully.", user.UserId);

            await _authService.SignInAsync(HttpContext, user.UserId, user.Email, user.Role.ToString());

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _authService.SignOutAsync(HttpContext);
            _logger.LogInformation("User {UserId} logged out.", userId);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogWarning("Access denied for User {UserId} attempting to access resource at {Path}.", userId, Request.Path.Value);
            return View();
        }
    }
}