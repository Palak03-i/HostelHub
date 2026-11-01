using Hostel_hub.ViewModels;
using Hostel_hub.Models;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        public Task<AdminDashboardViewModel> GetDashboardAsync(int? scopedHostelId) => throw new NotImplementedException();
        public Task<List<HostelOverviewRow>> GetAllHostelsOverviewAsync() => throw new NotImplementedException();
    }
}
