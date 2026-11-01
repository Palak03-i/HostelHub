using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hostel_hub.Services;
using Hostel_hub.ViewModels;

namespace Hostel_hub.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/MaintenanceStaff")]
    public class MaintenanceStaffController : Controller
    {
        private readonly IMaintenanceStaffService _staffService;
        private readonly IWardenContext _wardenContext;
        private readonly IWardenService _wardenService;
        private readonly ILogger<MaintenanceStaffController> _logger;

        public MaintenanceStaffController(IMaintenanceStaffService staffService, IWardenContext wardenContext, IWardenService wardenService, ILogger<MaintenanceStaffController> logger)
        {
            _staffService = staffService;
            _wardenContext = wardenContext;
            _wardenService = wardenService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var userId = GetCurrentUserId();

            var scopedHostelId =
                await _wardenContext.GetScopedHostelIdAsync(userId);

            bool isSuperAdmin = !scopedHostelId.HasValue;

            bool? isActive = null;
            if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                isActive = true;
            }
            else if (string.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                isActive = false;
            }

            var model = new StaffManagementViewModel
            {
                IsSuperAdmin = isSuperAdmin,
                StatusFilter = status,
                MaintenanceStaff =
                    await _staffService.GetAllAsync(
                        search,
                        scopedHostelId,
                        isActive)
            };

            if (isSuperAdmin)
            {
                model.Wardens =
                    await _wardenService.GetAllAsync();
            }

            return View(model);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var staff = await _staffService.GetByIdAsync(id);
            if (staff == null) return NotFound();

            var userId = GetCurrentUserId();
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(userId);
            if (scopedHostelId.HasValue && staff.HostelId != scopedHostelId.Value)
            {
                return Forbid();
            }

            ViewBag.CanManage = await CanManageAsync(staff.HostelId);
            return View(staff);
        }

        [HttpPost("Deactivate/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var staff = await _staffService.GetByIdAsync(id);
            if (staff == null) return NotFound();

            var userId = GetCurrentUserId();
            if (!await CanManageAsync(staff.HostelId))
            {
                _logger.LogWarning("Admin {AdminId} unauthorized to deactivate Maintenance staff {StaffId} in Hostel {HostelId}.", userId, id, staff.HostelId);
                return Forbid();
            }

            var (success, error) = await _staffService.DeactivateStaffAsync(id);
            if (!success)
            {
                _logger.LogWarning("Admin {AdminId} failed to deactivate Maintenance staff {StaffId}: {Error}", userId, id, error);
                TempData["StaffErrorMessage"] = error;
            }
            else
            {
                _logger.LogInformation("Maintenance staff {StaffId} was deactivated by Admin {AdminId}.", id, userId);
                TempData["StaffSuccessMessage"] = $"Staff member '{staff.FullName}' has been deactivated.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost("Reactivate/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var staff = await _staffService.GetByIdAsync(id);
            if (staff == null) return NotFound();

            var userId = GetCurrentUserId();
            if (!await CanManageAsync(staff.HostelId))
            {
                _logger.LogWarning("Admin {AdminId} unauthorized to reactivate Maintenance staff {StaffId} in Hostel {HostelId}.", userId, id, staff.HostelId);
                return Forbid();
            }

            var (success, error) = await _staffService.ReactivateStaffAsync(id);
            if (!success)
            {
                _logger.LogWarning("Admin {AdminId} failed to reactivate Maintenance staff {StaffId}: {Error}", userId, id, error);
                TempData["StaffErrorMessage"] = error;
            }
            else
            {
                _logger.LogInformation("Maintenance staff {StaffId} was reactivated by Admin {AdminId}.", id, userId);
                TempData["StaffSuccessMessage"] = $"Staff member '{staff.FullName}' has been reactivated.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            if (!scopedHostelId.HasValue)
            {
                TempData["StaffErrorMessage"] =
    "Only a Warden can create staff for their own hostel.";
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceStaffCreateViewModel model)
        {
            var adminId = GetCurrentUserId();
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(adminId);
            if (!scopedHostelId.HasValue)
            {
                TempData["ErrorMessage"] = "Only a Warden can create staff for their own hostel.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid) return View(model);

            var (success, error) = await _staffService.CreateAsync(model.Email, model.Password, model.FullName, model.PhoneNumber, model.Specialization, scopedHostelId.Value);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                return View(model);
            }

            _logger.LogInformation("Maintenance staff created by Admin {AdminId} for Hostel {HostelId}.", adminId, scopedHostelId.Value);
            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _staffService.GetByIdAsync(id);
            if (staff == null) return NotFound();

            if (!await CanManageAsync(staff.HostelId))
            {
                return Forbid();
            }

            return View(new MaintenanceStaffEditViewModel { FullName = staff.FullName, PhoneNumber = staff.PhoneNumber, Specialization = staff.Specialization });
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceStaffEditViewModel model)
        {
            var staff = await _staffService.GetByIdAsync(id);
            if (staff == null) return NotFound();

            var adminId = GetCurrentUserId();
            if (!await CanManageAsync(staff.HostelId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid) return View(model);

            bool success = await _staffService.UpdateAsync(id, model.FullName, model.PhoneNumber, model.Specialization);
            if (!success) return NotFound();

            _logger.LogInformation("Maintenance staff {StaffId} updated by Admin {AdminId}.", id, adminId);
            return RedirectToAction("Index");
        }

        // Unlike CanAccessHostelAsync elsewhere, this does NOT let Super Admin
        // bypass — only the Warden who actually owns this staff member's
        // hostel may manage them. A staff member with no HostelId assigned
        // yet (null) is unmanageable by anyone until one is set.
        private async Task<bool> CanManageAsync(int? staffHostelId)
        {
            if (!staffHostelId.HasValue) return false;

            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            return scopedHostelId.HasValue && scopedHostelId.Value == staffHostelId.Value;
        }

        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}