using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public class MessMenuService : IMessMenuService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MessMenuService> _logger;

        public MessMenuService(ApplicationDbContext context, ILogger<MessMenuService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<MessMenu>> GetMenusByHostelAsync(int? hostelId)
        {
            IQueryable<MessMenu> query = _context.MessMenus
                .Include(m => m.Hostel)
                .Include(m => m.Items);

            if (hostelId.HasValue)
            {
                query = query.Where(m => m.HostelId == hostelId.Value);
            }

            return await query.OrderByDescending(m => m.MenuDate).ToListAsync();
        }

        public async Task<MessMenu?> GetMenuByIdAsync(int menuId)
        {
            return await _context.MessMenus
                .Include(m => m.Hostel)
                .Include(m => m.Items)
                .FirstOrDefaultAsync(m => m.MessMenuId == menuId);
        }

        public async Task<MessMenu?> GetMenuByHostelAndDateAsync(int hostelId, DateOnly date)
        {
            return await _context.MessMenus
                .Include(m => m.Items)
                .FirstOrDefaultAsync(m => m.HostelId == hostelId && m.MenuDate == date);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateMenuAsync(MessMenu menu, List<MessMenuItem> items)
        {
            bool alreadyExists = await _context.MessMenus
                .AnyAsync(m => m.HostelId == menu.HostelId && m.MenuDate == menu.MenuDate);

            if (alreadyExists)
            {
                _logger.LogWarning("Failed to create menu: Menu for {MenuDate} already exists in Hostel {HostelId}.", menu.MenuDate, menu.HostelId);
                return (false, $"A menu for {menu.MenuDate:d} already exists for this hostel. Edit it instead of creating a new one.");
            }

            menu.Items = items;
            _context.MessMenus.Add(menu);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Mess menu {MenuId} created for Hostel {HostelId} on {MenuDate}.", menu.MessMenuId, menu.HostelId, menu.MenuDate);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateMenuAsync(int menuId, MessMenu updated, List<MessMenuItem> items)
        {
            var menu = await _context.MessMenus
                .Include(m => m.Items)
                .FirstOrDefaultAsync(m => m.MessMenuId == menuId);

            if (menu == null)
            {
                _logger.LogWarning("Failed to update menu: Menu {MenuId} not found.", menuId);
                return (false, "Menu not found.");
            }

            menu.IsBreakfastAvailable = updated.IsBreakfastAvailable;
            menu.IsLunchAvailable = updated.IsLunchAvailable;
            menu.IsDinnerAvailable = updated.IsDinnerAvailable;

            // Replace the dish list wholesale — simpler and safer than
            // diffing old vs new items for a college-project scope, and
            // MessMenuItem has no data worth preserving across an edit
            // (no history/audit trail hangs off individual dish rows).
            _context.MessMenuItems.RemoveRange(menu.Items);
            foreach (var item in items)
            {
                item.MessMenuId = menu.MessMenuId;
                _context.MessMenuItems.Add(item);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Mess menu {MenuId} updated.", menuId);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteMenuAsync(int menuId)
        {
            var menu = await _context.MessMenus
                .FirstOrDefaultAsync(m => m.MessMenuId == menuId);

            if (menu == null)
            {
                _logger.LogWarning("Failed to delete menu: Menu {MenuId} not found.", menuId);
                return (false, "Menu not found.");
            }

            bool hasSelections = await _context.MealSelections
                .AnyAsync(s => s.MessMenuId == menuId);

            if (hasSelections)
            {
                _logger.LogWarning("Failed to delete menu {MenuId}: Students have active meal selections.", menuId);
                return (false, "Cannot delete this menu — students have already made meal selections against it.");
            }

            _context.MessMenus.Remove(menu);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Mess menu {MenuId} deleted.", menuId);

            return (true, null);
        }
        public async Task<int> DeletePastMenusAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var pastMenus = await _context.MessMenus
                .Where(m => m.MenuDate < today)
                .ToListAsync();

            if (pastMenus.Count == 0)
            {
                return 0;
            }

            var pastMenuIds = pastMenus.Select(m => m.MessMenuId).ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            // MealSelection -> MessMenu is Restrict at the DB level, so the
            // selections must be removed first, deliberately, before the
            // menus themselves can be removed. This is the one place in
            // the project where we intentionally discard history rather
            // than protect it, per project decision.
            var selectionsToRemove = await _context.MealSelections
                .Where(s => pastMenuIds.Contains(s.MessMenuId))
                .ToListAsync();

            _context.MealSelections.RemoveRange(selectionsToRemove);
            _context.MessMenus.RemoveRange(pastMenus);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return pastMenus.Count;
        }
    }
}