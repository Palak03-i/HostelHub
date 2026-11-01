using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public interface IMaintenanceStaffService
    {
        Task<List<MaintenanceStaff>> GetAllAsync(string? search, int? hostelId, bool? isActive = null);
        Task<MaintenanceStaff?> GetByIdAsync(int staffId);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(string email, string password, string fullName, string? phoneNumber, StaffSpecialization specialization, int hostelId);
        Task<bool> UpdateAsync(int staffId, string fullName, string? phoneNumber, StaffSpecialization specialization);
        Task<(bool Success, string? ErrorMessage)> DeactivateStaffAsync(int staffId);
        Task<(bool Success, string? ErrorMessage)> ReactivateStaffAsync(int staffId);
        Task<bool> IsStaffActiveAsync(int staffId);
    }
}