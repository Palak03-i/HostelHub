using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class MaintenanceStaffService : IMaintenanceStaffService
    {
        public Task<List<MaintenanceStaff>> GetAllAsync(string? search, int? hostelId, bool? isActive = null) => throw new NotImplementedException();
        public Task<MaintenanceStaff?> GetByIdAsync(int staffId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> CreateAsync(string email, string password, string fullName, string? phoneNumber, StaffSpecialization specialization, int hostelId) => throw new NotImplementedException();
        public Task<bool> UpdateAsync(int staffId, string fullName, string? phoneNumber, StaffSpecialization specialization) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> DeactivateStaffAsync(int staffId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> ReactivateStaffAsync(int staffId) => throw new NotImplementedException();
        public Task<bool> IsStaffActiveAsync(int staffId) => throw new NotImplementedException();
    }
}
