namespace Hostel_hub.Services
{
    public interface IWardenContext
    {
        Task<int?> GetScopedHostelIdAsync(int userId);
        Task<bool> IsSuperAdminAsync(int userId);
    }
}