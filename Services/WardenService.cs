using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public class WardenService : IWardenService
    {
        private readonly ApplicationDbContext _context;

        public WardenService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Warden>> GetAllAsync()
        {
            return await _context.Wardens
                .Include(w => w.User)
                .Include(w => w.Hostel)
                .OrderBy(w => w.FullName)
                .ToListAsync();
        }
    }
}