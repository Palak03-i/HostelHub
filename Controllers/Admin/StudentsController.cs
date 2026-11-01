using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hostel_hub.Services;
using System.Security.Claims;

namespace Hostel_hub.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Students")]
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IWardenContext _wardenContext;
        private readonly IHostelService _hostelService;

        public StudentsController(IStudentService studentService, IWardenContext wardenContext, IHostelService hostelService)
        {
            _studentService = studentService;
            _wardenContext = wardenContext;
            _hostelService = hostelService;

        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, int? hostelId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            var scopedHostelId =
                await _wardenContext.GetScopedHostelIdAsync(userId);
            ViewBag.IsSuperAdmin =
    await _wardenContext.IsSuperAdminAsync(userId);

            int? effectiveHostelId;

            if (scopedHostelId.HasValue)
            {
                // Warden: always force their own hostel
                effectiveHostelId = scopedHostelId.Value;
            }
            else
            {
                // Super Admin: may view all or filter by hostel
                effectiveHostelId = hostelId;
            }

            var students =
                await _studentService.GetAllStudentsAsync(
                    search,
                    effectiveHostelId);

            return View(students);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            var scopedHostelId =
                await _wardenContext.GetScopedHostelIdAsync(userId);
            ViewBag.IsSuperAdmin =
    await _wardenContext.IsSuperAdminAsync(userId);

            if (scopedHostelId.HasValue &&
                student.HostelId != scopedHostelId.Value)
            {
                return Forbid();
            }

            return View(student);
        }
        [HttpGet("AssignHostel/{id}")]
        public async Task<IActionResult> AssignHostel(int id)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            // Only the real Super Admin can assign a student's hostel.
            bool isSuperAdmin =
                await _wardenContext.IsSuperAdminAsync(userId);

            if (!isSuperAdmin)
            {
                return Forbid();
            }

            var student =
                await _studentService.GetStudentByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            // This workflow is only for initial hostel assignment.
            if (student.HostelId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "This student is already assigned to a hostel.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = student.StudentId });
            }

            var hostels =
                await _hostelService.GetAllHostelsAsync();

            ViewBag.Hostels = hostels;

            return View(student);
        }


        [HttpPost("AssignHostel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignHostel(
            int id,
            int hostelId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            bool isSuperAdmin =
                await _wardenContext.IsSuperAdminAsync(userId);

            if (!isSuperAdmin)
            {
                return Forbid();
            }

            var student =
                await _studentService.GetStudentByIdAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            if (student.HostelId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "This student is already assigned to a hostel.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = student.StudentId });
            }

            var hostel =
                await _hostelService.GetHostelByIdAsync(hostelId);

            if (hostel == null)
            {
                TempData["ErrorMessage"] =
                    "The selected hostel does not exist.";

                ViewBag.Hostels =
                    await _hostelService.GetAllHostelsAsync();

                return View(student);
            }

            bool assigned =
                await _studentService.AssignHostelAsync(
                    student.StudentId,
                    hostelId);

            if (!assigned)
            {
                TempData["ErrorMessage"] =
                    "Hostel assignment could not be completed.";

                ViewBag.Hostels =
                    await _hostelService.GetAllHostelsAsync();

                return View(student);
            }

            TempData["SuccessMessage"] =
                $"{student.FullName} has been assigned to {hostel.Name}.";

            return RedirectToAction(
                nameof(Details),
                new { id = student.StudentId });
        }
    }
}