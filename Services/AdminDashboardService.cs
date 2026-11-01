using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;
using Hostel_hub.ViewModels;

namespace Hostel_hub.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync(int? scopedHostelId)
        {
            var model = new AdminDashboardViewModel();

            // --- Students ---
            var studentsQuery = _context.Students.AsQueryable();
            if (scopedHostelId.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.HostelId == scopedHostelId.Value);
            }
            model.TotalStudents = await studentsQuery.CountAsync();

            // --- Hostels ---
            model.TotalHostels = scopedHostelId.HasValue
                ? 1
                : await _context.Hostels.CountAsync();

            // --- Rooms ---
            var roomsQuery = _context.Rooms.AsQueryable();
            if (scopedHostelId.HasValue)
            {
                roomsQuery = roomsQuery.Where(r => r.HostelId == scopedHostelId.Value);
            }
            model.TotalRooms = await roomsQuery.CountAsync();
            model.FullRooms = await roomsQuery.CountAsync(r => r.CurrentOccupancy >= r.Capacity);
            model.AvailableRooms = await roomsQuery.CountAsync(r => r.CurrentOccupancy < r.Capacity);
            model.OccupiedRooms = await roomsQuery.CountAsync(r => r.CurrentOccupancy > 0);

            // --- Complaints (scoped via Student.HostelId, same as Admin/ComplaintsController) ---
            var complaintsQuery = _context.Complaints.AsQueryable();
            if (scopedHostelId.HasValue)
            {
                complaintsQuery = complaintsQuery.Where(c => c.Student!.HostelId == scopedHostelId.Value);
            }
            model.TotalComplaints = await complaintsQuery.CountAsync();
            model.PendingComplaints = await complaintsQuery.CountAsync(c => c.Status == ComplaintStatus.Pending);
            model.AssignedComplaints = await complaintsQuery.CountAsync(c => c.Status == ComplaintStatus.Assigned);
            model.InProgressComplaints = await complaintsQuery.CountAsync(c => c.Status == ComplaintStatus.InProgress);
            model.ResolvedComplaints = await complaintsQuery.CountAsync(c => c.Status == ComplaintStatus.Resolved);
            model.HighPriorityComplaints = await complaintsQuery.CountAsync(c => c.Priority == ComplaintPriority.High);
            model.MediumPriorityComplaints = await complaintsQuery.CountAsync(c => c.Priority == ComplaintPriority.Medium);
            model.LowPriorityComplaints = await complaintsQuery.CountAsync(c => c.Priority == ComplaintPriority.Low);

            // --- Mess: today's Take counts, grouped by meal, in one query ---
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayMenusQuery = _context.MessMenus.Where(m => m.MenuDate == today);
            if (scopedHostelId.HasValue)
            {
                todayMenusQuery = todayMenusQuery.Where(m => m.HostelId == scopedHostelId.Value);
            }
            var todayMenuIds = await todayMenusQuery.Select(m => m.MessMenuId).ToListAsync();

            var mealCounts = await _context.MealSelections
                .Where(s => todayMenuIds.Contains(s.MessMenuId) && s.Status == MealSelectionStatus.Take)
                .GroupBy(s => s.MealType)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            model.TodayBreakfastTaking = mealCounts.FirstOrDefault(m => m.Key == MealType.Breakfast)?.Count ?? 0;
            model.TodayLunchTaking = mealCounts.FirstOrDefault(m => m.Key == MealType.Lunch)?.Count ?? 0;
            model.TodayDinnerTaking = mealCounts.FirstOrDefault(m => m.Key == MealType.Dinner)?.Count ?? 0;

            // --- Announcements (not hostel-scoped, same as the Feedback module) ---
            var announcementsQuery = _context.Announcements
                .Where(a => a.IsActive && a.PublishDate <= today && a.ExpiryDate >= today);
                 if (scopedHostelId.HasValue)
                 {
                      announcementsQuery = announcementsQuery
                      .Where(a => a.HostelId == null || a.HostelId == scopedHostelId.Value);
                 }

            model.ActiveAnnouncementsCount =await announcementsQuery.CountAsync();

            return model;
        }
        public async Task<List<HostelOverviewRow>> GetAllHostelsOverviewAsync()
        {
            var hostels = await _context.Hostels.ToListAsync();

            // Grouped per-hostel queries — a constant number of round trips
            // regardless of how many hostels exist, instead of looping and
            // querying once per hostel (which would be an N+1 pattern).
            var studentCounts = await _context.Students
                .Where(s => s.HostelId != null)
                .GroupBy(s => s.HostelId)
                .Select(g => new { HostelId = g.Key!.Value, Count = g.Count() })
                .ToListAsync();

            var roomStats = await _context.Rooms
                .GroupBy(r => r.HostelId)
                .Select(g => new
                {
                    HostelId = g.Key,
                    Total = g.Count(),
                    Full = g.Count(r => r.CurrentOccupancy >= r.Capacity),
                    Occupied = g.Count(r => r.CurrentOccupancy > 0)
                })
                .ToListAsync();

            var complaintStats = await _context.Complaints
                .Where(c => c.Student!.HostelId != null)
                .GroupBy(c => c.Student!.HostelId)
                .Select(g => new
                {
                    HostelId = g.Key!.Value,
                    Open = g.Count(c => c.Status != ComplaintStatus.Resolved),
                    Resolved = g.Count(c => c.Status == ComplaintStatus.Resolved)
                })
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayMenus = await _context.MessMenus
                .Where(m => m.MenuDate == today)
                .ToListAsync();
            var menuIdToHostel = todayMenus.ToDictionary(m => m.MessMenuId, m => m.HostelId);
            var menuIds = todayMenus.Select(m => m.MessMenuId).ToList();

            var takeCountsByHostel = (await _context.MealSelections
                    .Where(s => menuIds.Contains(s.MessMenuId) && s.Status == MealSelectionStatus.Take)
                    .ToListAsync())
                .GroupBy(s => menuIdToHostel[s.MessMenuId])
                .ToDictionary(g => g.Key, g => g.Count());

            return hostels.Select(h => new HostelOverviewRow
            {
                HostelId = h.HostelId,
                Name = h.Name,
                TotalStudents = studentCounts.FirstOrDefault(s => s.HostelId == h.HostelId)?.Count ?? 0,
                TotalRooms = roomStats.FirstOrDefault(r => r.HostelId == h.HostelId)?.Total ?? 0,
                OccupiedRooms = roomStats.FirstOrDefault(r => r.HostelId == h.HostelId)?.Occupied ?? 0,
                FullRooms = roomStats.FirstOrDefault(r => r.HostelId == h.HostelId)?.Full ?? 0,
                OpenComplaints = complaintStats.FirstOrDefault(c => c.HostelId == h.HostelId)?.Open ?? 0,
                ResolvedComplaints = complaintStats.FirstOrDefault(c => c.HostelId == h.HostelId)?.Resolved ?? 0,
                TodayMealsTaking = takeCountsByHostel.TryGetValue(h.HostelId, out var count) ? count : 0
            }).ToList();
        }
    }
}