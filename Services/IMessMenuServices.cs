using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public interface IMessMenuService
    {
        Task<List<MessMenu>> GetMenusByHostelAsync(int? hostelId);
        Task<MessMenu?> GetMenuByIdAsync(int menuId);
        Task<MessMenu?> GetMenuByHostelAndDateAsync(int hostelId, DateOnly date);
        Task<(bool Success, string? ErrorMessage)> CreateMenuAsync(MessMenu menu, List<MessMenuItem> items);
        Task<(bool Success, string? ErrorMessage)> UpdateMenuAsync(int menuId, MessMenu updated, List<MessMenuItem> items);
        Task<(bool Success, string? ErrorMessage)> DeleteMenuAsync(int menuId);
        Task<int> DeletePastMenusAsync();
    }
}