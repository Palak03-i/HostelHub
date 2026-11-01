using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public class HostelService : IHostelService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HostelService> _logger;

        public HostelService(ApplicationDbContext context, ILogger<HostelService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Hostel>> GetAllHostelsAsync()
        {
            return await _context.Hostels
                .Include(h => h.Rooms)
                .OrderBy(h => h.Name)
                .ToListAsync();
        }

        public async Task<Hostel?> GetHostelByIdAsync(int hostelId)
        {
            return await _context.Hostels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HostelId == hostelId);
        }

        public async Task CreateHostelAsync(Hostel hostel)
        {
            _context.Hostels.Add(hostel);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Hostel {HostelId} created with name '{HostelName}'.", hostel.HostelId, hostel.Name);
        }

        public async Task<bool> UpdateHostelAsync(int hostelId, string name, HostelType type)
        {
            var hostel = await _context.Hostels.FirstOrDefaultAsync(h => h.HostelId == hostelId);

            if (hostel == null)
            {
                _logger.LogWarning("Failed to update hostel: Hostel {HostelId} not found.", hostelId);
                return false;
            }

            hostel.Name = name;
            hostel.Type = type;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Hostel {HostelId} updated with name '{HostelName}'.", hostelId, name);
            return true;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteHostelAsync(int hostelId)
        {
            var hostel = await _context.Hostels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.HostelId == hostelId);

            if (hostel == null)
            {
                _logger.LogWarning("Failed to delete hostel: Hostel {HostelId} not found.", hostelId);
                return (false, "Hostel not found.");
            }

            if (hostel.Rooms.Any())
            {
                _logger.LogWarning("Failed to delete Hostel {HostelId}: Has active rooms.", hostelId);
                return (false, $"Cannot delete '{hostel.Name}' — it still has {hostel.Rooms.Count} room(s). Remove or reassign its rooms first.");
            }
            bool hasAssignedWarden = await _context.Wardens.AnyAsync(w => w.HostelId == hostelId);

            if (hasAssignedWarden)
            {
                _logger.LogWarning("Failed to delete Hostel {HostelId}: Has assigned warden.", hostelId);
                return (
                    false,
                    $"Cannot delete '{hostel.Name}' — a Warden is still assigned to this hostel."
                );
            }

            if (await _context.Students.AnyAsync(s => s.HostelId == hostelId) ||
                await _context.MaintenanceStaff.AnyAsync(s => s.HostelId == hostelId) ||
                await _context.MessMenus.AnyAsync(m => m.HostelId == hostelId) ||
                await _context.Announcements.AnyAsync(a => a.HostelId == hostelId))
                return (false, "This hostel still has linked students, staff, menus or announcements.");

            _context.Hostels.Remove(hostel);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Hostel {HostelId} deleted.", hostelId);
            return (true, null);
        }
    }
}