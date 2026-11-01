using Hostel_hub.Data;
using Hostel_hub.Models;
using Hostel_hub.Services;
using Hostel_hub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hostel_hub.Controllers
{
    [Authorize(Roles = "MaintenanceStaff")]
    public class MaintenanceController : Controller
    {
        private readonly IComplaintService _complaintService;
        private readonly ApplicationDbContext _context;

        public MaintenanceController(IComplaintService complaintService, ApplicationDbContext context)
        {
            _complaintService = complaintService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> AssignedComplaints([FromQuery] ComplaintFilterViewModel filter)
        {
            var staffId = await GetCurrentStaffIdAsync();
            if (staffId == null) return Forbid();

            ViewBag.Filter = filter;

            if (filter.IsDateRangeInvalid)
            {
                ViewBag.DateErrorMessage = "From Date cannot be later than To Date.";
                return View(new List<Complaint>());
            }

            var complaints = await _complaintService.GetAssignedComplaintsAsync(staffId.Value, filter);
            return View(complaints);
        }

        [HttpGet]
        public async Task<IActionResult> ComplaintDetails(int id)
        {
            var staffId = await GetCurrentStaffIdAsync();
            if (staffId == null) return Forbid();

            var complaint = await _complaintService.GetComplaintByIdAsync(id);
            if (complaint == null || complaint.AssignedStaffId != staffId.Value)
            {
                return Forbid();
            }

            ViewBag.History = await _complaintService.GetHistoryAsync(id);
            return View(complaint);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int complaintId, ComplaintStatus newStatus)
        {
            var staffId = await GetCurrentStaffIdAsync();
            if (staffId == null) return Forbid();

            var complaint = await _complaintService.GetComplaintByIdAsync(complaintId);
            if (complaint == null || complaint.AssignedStaffId != staffId.Value)
            {
                return Forbid();
            }

            var (success, error) = await _complaintService.UpdateStatusAsync(complaintId, newStatus, "MaintenanceStaff", null);
            if (!success) TempData["ErrorMessage"] = error;

            return RedirectToAction("ComplaintDetails", new { id = complaintId });
        }

        private async Task<int?> GetCurrentStaffIdAsync()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            int userId = int.Parse(userIdClaim);
            var staff = await _context.MaintenanceStaff.FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
            return staff?.MaintenanceStaffId;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var staffId = await GetCurrentStaffIdAsync();
            if (staffId == null) return Forbid();

            var staff = await _context.MaintenanceStaff.FirstOrDefaultAsync(s => s.MaintenanceStaffId == staffId.Value);
            if (staff == null) return NotFound();

            var assigned = await _complaintService.GetAssignedComplaintsAsync(staff.MaintenanceStaffId);

            var model = new StaffDashboardViewModel
            {
                FullName = staff.FullName,
                Specialization = staff.Specialization,
                TotalAssigned = assigned.Count,
                PendingCount = assigned.Count(c => c.Status == ComplaintStatus.Assigned),
                InProgressCount = assigned.Count(c => c.Status == ComplaintStatus.InProgress),
                RecentlyResolved = assigned
                    .Where(c => c.Status == ComplaintStatus.Resolved)
                    .OrderByDescending(c => c.ResolvedDate)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }
    }
}