using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hostel_hub.Models;
using Hostel_hub.Services;
using Hostel_hub.ViewModels;

namespace Hostel_hub.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Announcements")]
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IHostelService _hostelService;
        private readonly IWardenContext _wardenContext;

        public AnnouncementsController(IAnnouncementService announcementService, IHostelService hostelService, IWardenContext wardenContext)
        {
            _announcementService = announcementService;
            _hostelService = hostelService;
            _wardenContext = wardenContext;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            var announcements = await _announcementService.GetAllAnnouncementsAsync(scopedHostelId);
            return View(announcements);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await PopulateHostelPickerAsync();
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementViewModel model)
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());

            if (!ModelState.IsValid)
            {
                await PopulateHostelPickerAsync();
                return View(model);
            }

            // A Warden's HostelId is never trusted from the form — always forced to their own hostel.
            // Super Admin's selection IS trusted (null = Global, or a specific hostel).
            int? hostelId = scopedHostelId.HasValue ? scopedHostelId.Value : model.HostelId;

            var announcement = new Announcement
            {
                Title = model.Title,
                Content = model.Content,
                PublishDate = model.PublishDate,
                ExpiryDate = model.ExpiryDate,
                IsImportant = model.IsImportant,
                HostelId = hostelId,
                PostedByUserId = GetCurrentUserId()
            };

            var (success, errorMessage) = await _announcementService.CreateAnnouncementAsync(announcement);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage!);
                await PopulateHostelPickerAsync();
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var announcement = await _announcementService.GetByIdAsync(id);
            if (announcement == null) return NotFound();

            if (!await CanManageAnnouncementAsync(announcement.HostelId))
            {
                return Forbid();
            }

            var model = new AnnouncementViewModel
            {
                Title = announcement.Title,
                Content = announcement.Content,
                PublishDate = announcement.PublishDate,
                ExpiryDate = announcement.ExpiryDate,
                IsImportant = announcement.IsImportant
            };

            ViewBag.AnnouncementId = id;
            ViewBag.HostelLabel = announcement.HostelId.HasValue ? announcement.Hostel?.Name : "Global (All Hostels)";
            return View(model);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnnouncementViewModel model)
        {
            var announcement = await _announcementService.GetByIdAsync(id);
            if (announcement == null) return NotFound();

            if (!await CanManageAnnouncementAsync(announcement.HostelId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AnnouncementId = id;
                ViewBag.HostelLabel = announcement.HostelId.HasValue ? announcement.Hostel?.Name : "Global (All Hostels)";
                return View(model);
            }

            var updated = new Announcement
            {
                Title = model.Title,
                Content = model.Content,
                PublishDate = model.PublishDate,
                ExpiryDate = model.ExpiryDate,
                IsImportant = model.IsImportant
                // HostelId intentionally untouched — ownership can't be reassigned via Edit.
            };

            var (success, errorMessage) = await _announcementService.UpdateAnnouncementAsync(id, updated);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage!);
                ViewBag.AnnouncementId = id;
                ViewBag.HostelLabel = announcement.HostelId.HasValue ? announcement.Hostel?.Name : "Global (All Hostels)";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost("Deactivate/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var announcement = await _announcementService.GetByIdAsync(id);
            if (announcement == null) return NotFound();

            if (!await CanManageAnnouncementAsync(announcement.HostelId))
            {
                return Forbid();
            }

            var (success, errorMessage) = await _announcementService.DeactivateAnnouncementAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("Index");
        }

        private async Task PopulateHostelPickerAsync()
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            ViewBag.IsSuperAdmin = !scopedHostelId.HasValue;

            if (!scopedHostelId.HasValue)
            {
                ViewBag.Hostels = await _hostelService.GetAllHostelsAsync();
            }
        }

        private async Task<bool> CanManageAnnouncementAsync(int? announcementHostelId)
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());

            if (!announcementHostelId.HasValue)
            {
                // Global announcement: only Super Admin (no scoped hostel) may manage it.
                return !scopedHostelId.HasValue;
            }

            // Hostel-specific: only the matching Warden may manage it.
            return scopedHostelId.HasValue && scopedHostelId.Value == announcementHostelId.Value;
        }

        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}