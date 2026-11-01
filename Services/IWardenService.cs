using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public interface IWardenService
    {
        Task<List<Warden>> GetAllAsync();
    }
}