using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hostel_hub.Models;
using Hostel_hub.Services;
using Hostel_hub.ViewModels;

namespace Hostel_hub.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/MessMenus")]
    public class MessMenusController : Controller
    {
        private readonly IMessMenuService _messMenuService;
        private readonly IMealSelectionService _mealSelectionService;
        private readonly IHostelService _hostelService;
        private readonly IWardenContext _wardenContext;

        public MessMenusController(IMessMenuService messMenuService, IMealSelectionService mealSelectionService, IHostelService hostelService, IWardenContext wardenContext)
        {
            _messMenuService = messMenuService;
            _mealSelectionService = mealSelectionService;
            _hostelService = hostelService;
            _wardenContext = wardenContext;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            int deletedCount = await _messMenuService.DeletePastMenusAsync();
            if (deletedCount > 0)
            {
                TempData["InfoMessage"] = $"{deletedCount} past menu(s) were automatically removed.";
            }

            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            var menus = await _messMenuService.GetMenusByHostelAsync(scopedHostelId);
            return View(menus);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var menu = await _messMenuService.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            if (!await CanAccessHostelAsync(menu.HostelId))
            {
                return Forbid();
            }

            return View(menu);
        }
        [HttpGet("Participation/{id}")]
        public async Task<IActionResult> Participation(int id)
        {
            var report = await _mealSelectionService.GetParticipationReportAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            if (!await CanAccessHostelAsync(report.HostelId))
            {
                return Forbid();
            }

            return View(report);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            if (!scopedHostelId.HasValue)
            {
                TempData["ErrorMessage"] = "Only a Warden can create a menu for their own hostel.";
                return RedirectToAction("Index");
            }

            await PopulateHostelDropdownAsync();
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MessMenuViewModel model)
        {
            if (!await CanManageAsync(model.HostelId))
            {
                return Forbid();
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (model.MenuDate < today)
            {
                ModelState.AddModelError(
                    nameof(model.MenuDate),
                    "Mess menu cannot be created for a past date.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateHostelDropdownAsync();
                return View(model);
            }

            var menu = new MessMenu
            {
                HostelId = model.HostelId,
                MenuDate = model.MenuDate,
                IsBreakfastAvailable = model.IsBreakfastAvailable,
                IsLunchAvailable = model.IsLunchAvailable,
                IsDinnerAvailable = model.IsDinnerAvailable,
                CreatedByUserId = GetCurrentUserId()
            };

            var items = ParseItems(model.BreakfastItemsText, MealType.Breakfast)
                .Concat(ParseItems(model.LunchItemsText, MealType.Lunch))
                .Concat(ParseItems(model.DinnerItemsText, MealType.Dinner))
                .ToList();

            var (success, errorMessage) = await _messMenuService.CreateMenuAsync(menu, items);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage!);
                await PopulateHostelDropdownAsync();
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var menu = await _messMenuService.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(menu.HostelId))
            {
                return Forbid();
            }

            var model = new MessMenuViewModel
            {
                HostelId = menu.HostelId,
                MenuDate = menu.MenuDate,
                IsBreakfastAvailable = menu.IsBreakfastAvailable,
                IsLunchAvailable = menu.IsLunchAvailable,
                IsDinnerAvailable = menu.IsDinnerAvailable,
                BreakfastItemsText = string.Join("\n", menu.Items.Where(i => i.MealType == MealType.Breakfast).Select(i => i.ItemName)),
                LunchItemsText = string.Join("\n", menu.Items.Where(i => i.MealType == MealType.Lunch).Select(i => i.ItemName)),
                DinnerItemsText = string.Join("\n", menu.Items.Where(i => i.MealType == MealType.Dinner).Select(i => i.ItemName))
            };

            ViewBag.MenuId = id;
            await PopulateHostelDropdownAsync();
            return View(model);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MessMenuViewModel model)
        {
            if (!await CanManageAsync(model.HostelId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MenuId = id;
                await PopulateHostelDropdownAsync();
                return View(model);
            }

            var updated = new MessMenu
            {
                IsBreakfastAvailable = model.IsBreakfastAvailable,
                IsLunchAvailable = model.IsLunchAvailable,
                IsDinnerAvailable = model.IsDinnerAvailable
            };

            var items = ParseItems(model.BreakfastItemsText, MealType.Breakfast)
                .Concat(ParseItems(model.LunchItemsText, MealType.Lunch))
                .Concat(ParseItems(model.DinnerItemsText, MealType.Dinner))
                .ToList();

            var (success, errorMessage) = await _messMenuService.UpdateMenuAsync(id, updated, items);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var menu = await _messMenuService.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(menu.HostelId))
            {
                return Forbid();
            }

            var (success, errorMessage) = await _messMenuService.DeleteMenuAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("Index");
        }

        private static List<MessMenuItem> ParseItems(string? text, MealType mealType)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<MessMenuItem>();
            }

            return text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .Select(line => new MessMenuItem { MealType = mealType, ItemName = line })
                .ToList();
        }

        private async Task PopulateHostelDropdownAsync()
        {
            var hostels = await _hostelService.GetAllHostelsAsync();
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());

            if (scopedHostelId.HasValue)
            {
                hostels = hostels.Where(h => h.HostelId == scopedHostelId.Value).ToList();
            }

            ViewBag.Hostels = hostels;
        }

        private async Task<bool> CanAccessHostelAsync(int hostelId)
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            return !scopedHostelId.HasValue || scopedHostelId.Value == hostelId;
        }

        private async Task<bool> CanManageAsync(int hostelId)
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            return scopedHostelId.HasValue && scopedHostelId.Value == hostelId;
        }

        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

    }
}