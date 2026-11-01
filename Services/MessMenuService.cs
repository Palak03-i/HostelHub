using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class MessMenuService : IMessMenuService
    {
        public Task<List<MessMenu>> GetMenusByHostelAsync(int? hostelId) => throw new NotImplementedException();
        public Task<MessMenu?> GetMenuByIdAsync(int menuId) => throw new NotImplementedException();
        public Task<MessMenu?> GetMenuByHostelAndDateAsync(int hostelId, DateOnly date) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> CreateMenuAsync(MessMenu menu, List<MessMenuItem> items) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> UpdateMenuAsync(int menuId, MessMenu updated, List<MessMenuItem> items) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> DeleteMenuAsync(int menuId) => throw new NotImplementedException();
        public Task<int> DeletePastMenusAsync() => throw new NotImplementedException();
    }
}
