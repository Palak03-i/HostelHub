using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Hostel_hub.Data;
using Hostel_hub.Models;
using Hostel_hub.Services;
using Hostel_hub.ViewModels;

namespace Hostel_hub.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Hostels")]
    public class HostelsController : Controller
    {
        private readonly IHostelService _hostelService;
        private readonly IWardenContext _wardenContext;
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public HostelsController(IHostelService hostelService, IWardenContext wardenContext, ApplicationDbContext context, IAuthService authService)
        {
            _hostelService = hostelService;
            _wardenContext = wardenContext;
            _context = context;
            _authService = authService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(userId);

            var hostels = await _hostelService.GetAllHostelsAsync();

            if (scopedHostelId.HasValue)
            {
                hostels = hostels.Where(h => h.HostelId == scopedHostelId.Value).ToList();
            }

            return View(hostels);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            if (!await CanAccessHostelAsync(id))
            {
                return Forbid();
            }

            var hostel = await _hostelService.GetHostelByIdAsync(id);
            if (hostel == null)
            {
                return NotFound();
            }
            return View(hostel);
        }
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            if (!await _wardenContext.IsSuperAdminAsync(GetCurrentUserId()))
            {
                return Forbid();
            }

            var model = new CreateHostelWithWardenViewModel();

            return View(model);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHostelWithWardenViewModel model)
        {
            if (!await _wardenContext.IsSuperAdminAsync(GetCurrentUserId()))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string normalizedEmail = model.WardenEmail.Trim().ToLowerInvariant();
            string hostelName = model.HostelName.Trim();

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.WardenEmail),
                    "This email is already registered.");

                return View(model);
            }

            bool hostelExists = await _context.Hostels.AnyAsync(h => h.Name == hostelName);

            if (hostelExists)
            {
                ModelState.AddModelError(
                    nameof(model.HostelName),
                    "A hostel with this name already exists.");

                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Create Hostel
                var hostel = new Hostel
                {
                    Name = hostelName,
                    Type = model.HostelType
                };

                await _hostelService.CreateHostelAsync(hostel);

                // 2. Create Warden login account
                var user = new User
                {
                    Email = normalizedEmail,
                    PasswordHash = _authService.HashPassword(model.Password),
                    Role = UserRole.Admin
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 3. Create Warden profile and assign hostel
                var warden = new Warden
                {
                    UserId = user.UserId,
                    FullName = model.WardenFullName.Trim(),
                    HostelId = hostel.HostelId
                };

                _context.Wardens.Add(warden);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    "Hostel and Warden account created successfully.";

                return RedirectToAction("Index");
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create the hostel and Warden account. Please try again.");

                return View(model);
            }
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (!await _wardenContext.IsSuperAdminAsync(GetCurrentUserId()))
            {
                return Forbid();
            }

            var hostel = await _hostelService.GetHostelByIdAsync(id);

            if (hostel == null)
            {
                return NotFound();
            }

            var model = new HostelViewModel
            {
                Name = hostel.Name,
                Type = hostel.Type
            };

            return View(model);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HostelViewModel model)
        {
            if (!await _wardenContext.IsSuperAdminAsync(GetCurrentUserId()))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool success = await _hostelService.UpdateHostelAsync(
                id,
                model.Name,
                model.Type);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _wardenContext.IsSuperAdminAsync(GetCurrentUserId()))
            {
                return Forbid();
            }

            var (success, errorMessage) = await _hostelService.DeleteHostelAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("Index");
        }

        private async Task<bool> CanAccessHostelAsync(int hostelId)
        {
            int userId = GetCurrentUserId();
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(userId);

            return !scopedHostelId.HasValue || scopedHostelId.Value == hostelId;
        }

        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}