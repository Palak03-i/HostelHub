using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;

namespace Hostel_hub.Services
{
    public class WardenContext : IWardenContext
    {
        private readonly ApplicationDbContext _context;

        public WardenContext(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetScopedHostelIdAsync(int userId)
        {
            var warden = await _context.Wardens.FirstOrDefaultAsync(w => w.UserId == userId);
            return warden?.HostelId;
        }

        public async Task<bool> IsSuperAdminAsync(int userId)
        {
            bool hasWardenRecord = await _context.Wardens.AnyAsync(w => w.UserId == userId);
            return !hasWardenRecord && await _context.Users.AnyAsync(u =>
                u.UserId == userId && u.Role == Hostel_hub.Models.UserRole.Admin);
        }
    }
}