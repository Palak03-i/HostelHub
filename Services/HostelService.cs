using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class HostelService : IHostelService
    {
        public Task<List<Hostel>> GetAllHostelsAsync() => throw new NotImplementedException();
        public Task<Hostel?> GetHostelByIdAsync(int hostelId) => throw new NotImplementedException();
        public Task CreateHostelAsync(Hostel hostel) => throw new NotImplementedException();
        public Task<bool> UpdateHostelAsync(int hostelId, string name, HostelType type) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> DeleteHostelAsync(int hostelId) => throw new NotImplementedException();
    }
}
