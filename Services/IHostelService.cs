using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public interface IHostelService
    {
        Task<List<Hostel>> GetAllHostelsAsync();
        Task<Hostel?> GetHostelByIdAsync(int hostelId);
        Task CreateHostelAsync(Hostel hostel);
        Task<bool> UpdateHostelAsync(int hostelId, string name, HostelType type);
        Task<(bool Success, string? ErrorMessage)> DeleteHostelAsync(int hostelId);
    }
}