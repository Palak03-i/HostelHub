using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public class MaintenanceStaffService : IMaintenanceStaffService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<MaintenanceStaffService> _logger;

        public MaintenanceStaffService(ApplicationDbContext context, IAuthService authService, ILogger<MaintenanceStaffService> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        public async Task<List<MaintenanceStaff>> GetAllAsync(string? search, int? hostelId, bool? isActive = null)
        {
            IQueryable<MaintenanceStaff> query = _context.MaintenanceStaff
                .Include(m => m.User)
                .Include(m => m.Hostel);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.FullName.Contains(search));
            }

            if (hostelId.HasValue)
            {
                query = query.Where(m => m.HostelId == hostelId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            return await query.OrderBy(m => m.FullName).ToListAsync();
        }

        public async Task<MaintenanceStaff?> GetByIdAsync(int staffId)
        {
            return await _context.MaintenanceStaff
                .Include(m => m.User)
                .Include(m => m.Hostel)
                .FirstOrDefaultAsync(m => m.MaintenanceStaffId == staffId);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(string email, string password, string fullName, string? phoneNumber, StaffSpecialization specialization, int hostelId)
        {
            email = email.Trim().ToLowerInvariant();
            if (!Enum.IsDefined(specialization) || !await _context.Hostels.AnyAsync(h => h.HostelId == hostelId))
                return (false, "Select a valid hostel and specialization.");
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            if (emailExists)
            {
                _logger.LogWarning("Staff creation failed: Email {Email} is already registered.", email);
                return (false, "This email is already registered.");
            }

            var user = new User
            {
                Email = email,
                PasswordHash = _authService.HashPassword(password),
                Role = UserRole.MaintenanceStaff
            };
            _context.Users.Add(user);

            var staff = new MaintenanceStaff
            {
                User = user,
                HostelId = hostelId,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Specialization = specialization,
                IsActive = true
            };
            _context.MaintenanceStaff.Add(staff);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Maintenance staff {StaffId} created for Hostel {HostelId}.", staff.MaintenanceStaffId, hostelId);

            return (true, null);
        }

        public async Task<bool> UpdateAsync(int staffId, string fullName, string? phoneNumber, StaffSpecialization specialization)
        {
            var staff = await _context.MaintenanceStaff.FirstOrDefaultAsync(m => m.MaintenanceStaffId == staffId);
            if (staff == null)
            {
                _logger.LogWarning("Failed to update staff: Staff {StaffId} not found.", staffId);
                return false;
            }

            staff.FullName = fullName;
            staff.PhoneNumber = phoneNumber;
            staff.Specialization = specialization;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Maintenance staff {StaffId} updated successfully.", staffId);

            return true;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeactivateStaffAsync(int staffId)
        {
            var staff = await _context.MaintenanceStaff.FirstOrDefaultAsync(m => m.MaintenanceStaffId == staffId);
            if (staff == null)
            {
                _logger.LogWarning("Failed to deactivate staff: Staff {StaffId} not found.", staffId);
                return (false, "Maintenance staff not found.");
            }

            if (!staff.IsActive)
            {
                _logger.LogWarning("Failed to deactivate staff: Staff {StaffId} is already inactive.", staffId);
                return (false, "Staff member is already inactive.");
            }

            staff.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Maintenance staff {StaffId} was deactivated.", staffId);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> ReactivateStaffAsync(int staffId)
        {
            var staff = await _context.MaintenanceStaff.FirstOrDefaultAsync(m => m.MaintenanceStaffId == staffId);
            if (staff == null)
            {
                _logger.LogWarning("Failed to reactivate staff: Staff {StaffId} not found.", staffId);
                return (false, "Maintenance staff not found.");
            }

            if (staff.IsActive)
            {
                _logger.LogWarning("Failed to reactivate staff: Staff {StaffId} is already active.", staffId);
                return (false, "Staff member is already active.");
            }

            staff.IsActive = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Maintenance staff {StaffId} was reactivated.", staffId);

            return (true, null);
        }

        public async Task<bool> IsStaffActiveAsync(int staffId)
        {
            var staff = await _context.MaintenanceStaff.AsNoTracking().FirstOrDefaultAsync(m => m.MaintenanceStaffId == staffId);
            return staff != null && staff.IsActive;
        }
    }
}