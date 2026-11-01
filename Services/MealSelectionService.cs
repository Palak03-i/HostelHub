using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;
using Hostel_hub.ViewModels;

namespace Hostel_hub.Services
{
    public class MealSelectionService : IMealSelectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MealSelectionService> _logger;
        private readonly TimeProvider _clock;

        public MealSelectionService(ApplicationDbContext context, IConfiguration configuration, ILogger<MealSelectionService> logger, TimeProvider? clock = null)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _clock = clock ?? TimeProvider.System;
        }

        public async Task<List<MealSelection>> GetSelectionsForStudentAsync(int studentId, int menuId)
        {
            return await _context.MealSelections
                .Where(s => s.StudentId == studentId && s.MessMenuId == menuId)
                .ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> SelectMealAsync(int studentId, int menuId, MealType mealType, MealSelectionStatus status)
        {
            if (!Enum.IsDefined(mealType) || !Enum.IsDefined(status))
                return (false, "Invalid meal or selection status.");

            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
            {
                _logger.LogWarning("Meal selection failed: Student {StudentId} not found.", studentId);
                return (false, "Student not found.");
            }

            var menu = await _context.MessMenus.FirstOrDefaultAsync(m => m.MessMenuId == menuId);
            if (menu == null)
            {
                _logger.LogWarning("Meal selection failed: Menu {MenuId} not found.", menuId);
                return (false, "Menu not found.");
            }

            if (student.HostelId != menu.HostelId)
            {
                _logger.LogWarning("Meal selection failed: Student {StudentId} belongs to Hostel {StudentHostelId}, but Menu {MenuId} is for Hostel {MenuHostelId}.", studentId, student.HostelId, menuId, menu.HostelId);
                return (false, "You can only select meals from your own hostel's menu.");
            }

            bool mealIsAvailable = mealType switch
            {
                MealType.Breakfast => menu.IsBreakfastAvailable,
                MealType.Lunch => menu.IsLunchAvailable,
                MealType.Dinner => menu.IsDinnerAvailable,
                _ => false
            };

            if (!mealIsAvailable)
            {
                _logger.LogWarning("Meal selection failed: MealType {MealType} is unavailable for Menu {MenuId}.", mealType, menuId);
                return (false, $"{mealType} is not available for {menu.MenuDate:d}.");
            }

            if (!IsMealSelectionWithinCutoff(menu.MenuDate,mealType,out var cutoffDateTime))
            {
                _logger.LogWarning("Meal selection failed: Cutoff passed for MealType {MealType} on Menu {MenuId}.", mealType, menuId);
                return (false, $"Selection for {mealType} on {menu.MenuDate:d} closed at {cutoffDateTime:g}.");
            }

            var existing = await _context.MealSelections
                .FirstOrDefaultAsync(s => s.StudentId == studentId && s.MessMenuId == menuId && s.MealType == mealType);

            if (existing != null)
            {
                existing.Status = status;
                existing.SelectedAt = DateTime.UtcNow;
            }
            else
            {
                _context.MealSelections.Add(new MealSelection
                {
                    StudentId = studentId,
                    MessMenuId = menuId,
                    MealType = mealType,
                    Status = status
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Meal selection updated: Student {StudentId}, Menu {MenuId}, MealType {MealType}, Status {Status}.", studentId, menuId, mealType, status);

            return (true, null);
        }

        public async Task<MessParticipationViewModel?> GetParticipationReportAsync(int menuId)
        {
            var menu = await _context.MessMenus
                .Include(m => m.Hostel)
                .FirstOrDefaultAsync(m => m.MessMenuId == menuId);

            if (menu == null)
            {
                return null;
            }

            int totalStudents = await _context.Students
                .CountAsync(s => s.HostelId == menu.HostelId);

            var counts = await _context.MealSelections
                .Where(s => s.MessMenuId == menuId)
                .GroupBy(s => new { s.MealType, s.Status })
                .Select(g => new { g.Key.MealType, g.Key.Status, Count = g.Count() })
                .ToListAsync();

            var report = new MessParticipationViewModel
            {
                MenuId = menu.MessMenuId,
                HostelId = menu.HostelId,
                HostelName = menu.Hostel?.Name ?? string.Empty,
                MenuDate = menu.MenuDate,
                TotalStudents = totalStudents
            };

            foreach (var meal in new[] { MealType.Breakfast, MealType.Lunch, MealType.Dinner })
            {
                int taking = counts
                    .Where(c => c.MealType == meal && c.Status == MealSelectionStatus.Take)
                    .Sum(c => c.Count);
                int skipping = counts
                    .Where(c => c.MealType == meal && c.Status == MealSelectionStatus.Skip)
                    .Sum(c => c.Count);

                report.Rows.Add(new MealParticipationRow
                {
                    MealType = meal,
                    IsAvailable = meal switch
                    {
                        MealType.Breakfast => menu.IsBreakfastAvailable,
                        MealType.Lunch => menu.IsLunchAvailable,
                        MealType.Dinner => menu.IsDinnerAvailable,
                        _ => false
                    },
                    Taking = taking,
                    Skipping = skipping,
                    NoResponse = totalStudents - taking - skipping
                });
            }

            return report;
        }

        // Cutoff is relative to the meal's own date. Breakfast's cutoff sits
        // on the night BEFORE MenuDate (DaysBefore = 1); Lunch/Dinner use
        // MenuDate itself (DaysBefore = 0).
        public bool IsMealSelectionWithinCutoff(
    DateOnly menuDate,
    MealType mealType,
    out DateTime cutoffDateTime)
        {
            int daysBefore = _configuration.GetValue<int>(
                $"MealCutoffs:{mealType}:DaysBefore",
                0);

            string timeText =
                _configuration[$"MealCutoffs:{mealType}:Time"] ?? "23:59";

            if (!Enum.IsDefined(mealType) || daysBefore < 0 ||
                !TimeOnly.TryParseExact(timeText, "HH:mm", System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var cutoffTime))
                throw new InvalidOperationException("Meal cutoff must use HH:mm and nonnegative DaysBefore.");

            var cutoffDate = menuDate.AddDays(-daysBefore);

            cutoffDateTime =
                cutoffDate.ToDateTime(
                    cutoffTime);

            return _clock.GetLocalNow().DateTime <= cutoffDateTime;
        }
    }
}