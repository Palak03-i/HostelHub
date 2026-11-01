using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class WardenContext : IWardenContext
    {
        public Task<int?> GetScopedHostelIdAsync(int userId) => throw new NotImplementedException();
        public Task<bool> IsSuperAdminAsync(int userId) => throw new NotImplementedException();
    }
}
