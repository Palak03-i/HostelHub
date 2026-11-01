using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class MealSelectionService : IMealSelectionService
    {
        public Task<List<MealSelection>> GetSelectionsForStudentAsync(int studentId, int menuId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> SelectMealAsync(int studentId, int menuId, MealType mealType, MealSelectionStatus status) => throw new NotImplementedException();
        public Task<MessParticipationViewModel?> GetParticipationReportAsync(int menuId) => throw new NotImplementedException();
        public bool IsMealSelectionWithinCutoff(DateOnly menuDate,MealType mealType,out DateTime cutoffDateTime) => throw new NotImplementedException();
    }
}
