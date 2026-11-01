using Hostel_hub.Models;
using Hostel_hub.ViewModels;

namespace Hostel_hub.Services
{
    public interface IMealSelectionService
    {
        Task<List<MealSelection>> GetSelectionsForStudentAsync(int studentId, int menuId);
        Task<(bool Success, string? ErrorMessage)> SelectMealAsync(int studentId, int menuId, MealType mealType, MealSelectionStatus status);
        Task<MessParticipationViewModel?> GetParticipationReportAsync(int menuId);
        bool IsMealSelectionWithinCutoff(DateOnly menuDate,MealType mealType,out DateTime cutoffDateTime);
    }
}