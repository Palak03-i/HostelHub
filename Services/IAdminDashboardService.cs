using Hostel_hub.ViewModels;

namespace Hostel_hub.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync(int? scopedHostelId);
        Task<List<HostelOverviewRow>> GetAllHostelsOverviewAsync();
    }
}