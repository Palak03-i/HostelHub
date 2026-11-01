using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        public Task<List<Announcement>> GetActiveAnnouncementsAsync(int? hostelId) => throw new NotImplementedException();
        public Task<List<Announcement>> GetAllAnnouncementsAsync(int? scopedHostelId) => throw new NotImplementedException();
        public Task<Announcement?> GetByIdAsync(int announcementId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> CreateAnnouncementAsync(Announcement announcement) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> UpdateAnnouncementAsync(int announcementId, Announcement updated) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> DeactivateAnnouncementAsync(int announcementId) => throw new NotImplementedException();
    }
}
