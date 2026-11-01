using Hostel_hub.Data;
using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Microsoft.EntityFrameworkCore;
using static Hostel_hub.Models.Complaint;

namespace Hostel_hub.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComplaintService> _logger;

        // Defines exactly which status transitions are legal.
        private static readonly Dictionary<ComplaintStatus, List<ComplaintStatus>> AllowedTransitions = new()
        {
            { ComplaintStatus.Pending, new List<ComplaintStatus> { ComplaintStatus.Assigned } },
            { ComplaintStatus.Assigned, new List<ComplaintStatus> { ComplaintStatus.InProgress } },
            { ComplaintStatus.InProgress, new List<ComplaintStatus> { ComplaintStatus.Resolved } },
            { ComplaintStatus.Resolved, new List<ComplaintStatus>() }
        };

        public ComplaintService(ApplicationDbContext context, ILogger<ComplaintService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateComplaintAsync(int studentId, string title, string description, ComplaintCategory category)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
            {
                _logger.LogWarning("Failed to create complaint: Student {StudentId} not found.", studentId);
                return (false, "Student not found.");
            }
            if (!student.RoomId.HasValue)
            {
                _logger.LogWarning("Failed to create complaint: Student {StudentId} has no assigned room.", studentId);
                return (false, "You must have a room assigned before filing a complaint.");
            }

            var complaint = new Complaint
            {
                StudentId = studentId,
                Title = title,
                Description = description,
                Category = category,
                Priority = ComplaintPriority.Low,
                Status = ComplaintStatus.Pending
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            await AddHistoryAsync(complaint.ComplaintId, ComplaintStatus.Pending, ComplaintStatus.Pending, "Student", "Complaint created.");

            _logger.LogInformation("Complaint {ComplaintId} created by Student {StudentId}.", complaint.ComplaintId, studentId);

            return (true, null);
        }

        public async Task<List<Complaint>> GetComplaintsForStudentAsync(int studentId, ComplaintFilterViewModel filter)
        {
            IQueryable<Complaint> query = _context.Complaints
                .AsNoTracking()
                .Include(c => c.AssignedStaff)
                .Where(c => c.StudentId == studentId);

            query = ApplyFilters(query, filter);

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public Task<List<Complaint>> GetComplaintsForStudentAsync(int studentId)
        {
            return GetComplaintsForStudentAsync(studentId, new ComplaintFilterViewModel());
        }

        public async Task<List<Complaint>> GetAllComplaintsAsync(int? hostelId, ComplaintFilterViewModel filter)
        {
            IQueryable<Complaint> query = _context.Complaints
                .AsNoTracking()
                .Include(c => c.Student)
                .Include(c => c.AssignedStaff);

            if (hostelId.HasValue)
            {
                query = query.Where(c => c.Student!.HostelId == hostelId.Value);
            }

            query = ApplyFilters(query, filter);

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public Task<List<Complaint>> GetAllComplaintsAsync(int? hostelId, ComplaintStatus? status, ComplaintCategory? category, ComplaintPriority? priority)
        {
            return GetAllComplaintsAsync(hostelId, new ComplaintFilterViewModel
            {
                Status = status,
                Category = category,
                Priority = priority
            });
        }

        public async Task<List<Complaint>> GetAssignedComplaintsAsync(
    int staffId,
    ComplaintFilterViewModel filter)
        {
            IQueryable<Complaint> query = _context.Complaints
                .AsNoTracking()
                .Include(c => c.Student)
                .Where(c => c.AssignedStaffId == staffId);

            query = ApplyFilters(query, filter);

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public Task<List<Complaint>> GetAssignedComplaintsAsync(int staffId)
        {
            return GetAssignedComplaintsAsync(staffId, new ComplaintFilterViewModel());
        }

        private static IQueryable<Complaint> ApplyFilters(IQueryable<Complaint> query, ComplaintFilterViewModel filter)
        {
            if (filter == null) return query;

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim();
                var cleanIdStr = term.StartsWith("#") ? term.Substring(1).Trim() : term;
                bool isId = int.TryParse(cleanIdStr, out int complaintId);

                if (isId)
                {
                    query = query.Where(c => c.ComplaintId == complaintId ||
                                             c.Title.Contains(term) ||
                                             c.Description.Contains(term));
                }
                else
                {
                    query = query.Where(c => c.Title.Contains(term) ||
                                             c.Description.Contains(term));
                }
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(c => c.Status == filter.Status.Value);
            }

            if (filter.Category.HasValue)
            {
                query = query.Where(c => c.Category == filter.Category.Value);
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(c => c.Priority == filter.Priority.Value);
            }

            if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate.Value.Date > filter.ToDate.Value.Date)
            {
                // Invalid date range: condition cannot be satisfied
                query = query.Where(c => false);
            }
            else
            {
                if (filter.FromDate.HasValue)
                {
                    var from = filter.FromDate.Value.Date;
                    query = query.Where(c => c.CreatedAt >= from);
                }

                if (filter.ToDate.HasValue)
                {
                    var toExclusive = filter.ToDate.Value.Date.AddDays(1);
                    query = query.Where(c => c.CreatedAt < toExclusive);
                }
            }

            if (filter.StaffId.HasValue)
            {
                if (filter.StaffId.Value <= 0)
                {
                    query = query.Where(c => c.AssignedStaffId == null);
                }
                else
                {
                    query = query.Where(c => c.AssignedStaffId == filter.StaffId.Value);
                }
            }

            return query;
        }

        public async Task<Complaint?> GetComplaintByIdAsync(int complaintId)
        {
            return await _context.Complaints
                .Include(c => c.Student)
                .Include(c => c.AssignedStaff)
                .FirstOrDefaultAsync(c => c.ComplaintId == complaintId);
        }

        public async Task<List<ComplaintStatusHistory>> GetHistoryAsync(int complaintId)
        {
            return await _context.ComplaintStatusHistories
                .Where(h => h.ComplaintId == complaintId)
                .OrderBy(h => h.ChangedAt)
                .ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> SetPriorityAsync(int complaintId, ComplaintPriority priority, string changedBy)
        {
            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.ComplaintId == complaintId);
            if (complaint == null)
            {
                _logger.LogWarning("Failed to set priority: Complaint {ComplaintId} not found.", complaintId);
                return (false, "Complaint not found.");
            }

            if (!Enum.IsDefined(priority)) return (false, "Invalid priority.");
            complaint.Priority = priority;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Complaint {ComplaintId} priority changed to {Priority} by {ChangedBy}.", complaintId, priority, changedBy);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> AssignStaffAsync(int complaintId, int staffId, string changedBy)
        {
            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.ComplaintId == complaintId);
            if (complaint == null)
            {
                _logger.LogWarning("Failed to assign staff: Complaint {ComplaintId} not found.", complaintId);
                return (false, "Complaint not found.");
            }

            if (complaint.Status == ComplaintStatus.Resolved)
            {
                _logger.LogWarning("Failed to assign Complaint {ComplaintId}: Complaint is already resolved.", complaintId);
                return (false, "Cannot assign a resolved complaint.");
            }

            var staff = await _context.MaintenanceStaff.FirstOrDefaultAsync(s => s.MaintenanceStaffId == staffId);
            if (staff == null)
            {
                _logger.LogWarning("Failed to assign Complaint {ComplaintId}: Staff {StaffId} not found.", complaintId, staffId);
                return (false, "Staff member not found.");
            }

            if (!staff.IsActive)
            {
                _logger.LogWarning("Failed to assign Complaint {ComplaintId} to Staff {StaffId}: Staff is inactive.", complaintId, staffId);
                return (false, "Selected maintenance staff is inactive and cannot be assigned to a new complaint.");
            }

            var studentHostel = await _context.Students.Where(s => s.StudentId == complaint.StudentId)
                .Select(s => s.HostelId).FirstOrDefaultAsync();
            if (!studentHostel.HasValue || staff.HostelId != studentHostel)
                return (false, "Staff and complaint must belong to the same hostel.");

            var oldStatus = complaint.Status;
            complaint.AssignedStaffId = staffId;
            complaint.Status = ComplaintStatus.Assigned;
            await _context.SaveChangesAsync();

            await AddHistoryAsync(complaintId, oldStatus, ComplaintStatus.Assigned, changedBy, $"Assigned to {staff.FullName}.");

            _logger.LogInformation("Complaint {ComplaintId} assigned to Staff {StaffId} by {ChangedBy}.", complaintId, staffId, changedBy);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(int complaintId, ComplaintStatus newStatus, string changedBy, string? remarks)
        {
            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.ComplaintId == complaintId);
            if (complaint == null)
            {
                _logger.LogWarning("Failed to update status: Complaint {ComplaintId} not found.", complaintId);
                return (false, "Complaint not found.");
            }

            var oldStatus = complaint.Status;

            if (!AllowedTransitions.TryGetValue(oldStatus, out var allowed) || !allowed.Contains(newStatus))
            {
                _logger.LogWarning("Invalid status change attempted for Complaint {ComplaintId} from {OldStatus} to {NewStatus} by {ChangedBy}.", complaintId, oldStatus, newStatus, changedBy);
                return (false, $"Cannot change status from {oldStatus} to {newStatus}. Only {string.Join(", ", allowed ?? new List<ComplaintStatus>())} allowed.");
            }

            complaint.Status = newStatus;
            if (newStatus == ComplaintStatus.Resolved)
            {
                complaint.ResolvedDate = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            await AddHistoryAsync(complaintId, oldStatus, newStatus, changedBy, remarks);

            _logger.LogInformation("Complaint {ComplaintId} status changed from {OldStatus} to {NewStatus} by {ChangedBy}.", complaintId, oldStatus, newStatus, changedBy);

            if (newStatus == ComplaintStatus.Resolved)
            {
                _logger.LogInformation("Complaint {ComplaintId} resolved by Staff {StaffId}.", complaintId, complaint.AssignedStaffId);
            }

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateComplaintAsync(int complaintId, int studentId, string title, string description, ComplaintCategory category)
        {
            var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.ComplaintId == complaintId);
            if (complaint == null)
            {
                _logger.LogWarning("Failed to update complaint: Complaint {ComplaintId} not found.", complaintId);
                return (false, "Complaint not found.");
            }

            if (complaint.StudentId != studentId)
            {
                _logger.LogWarning("Unauthorized attempt by Student {StudentId} to edit Complaint {ComplaintId}.", studentId, complaintId);
                return (false, "You are not authorized to edit this complaint.");
            }

            if (complaint.Status != ComplaintStatus.Pending)
            {
                _logger.LogWarning("Attempt to edit Complaint {ComplaintId} with non-pending status {Status} by Student {StudentId}.", complaintId, complaint.Status, studentId);
                return (false, "Only complaints with 'Pending' status can be edited.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return (false, "Title is required.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return (false, "Description is required.");
            }

            complaint.Title = title.Trim();
            complaint.Description = description.Trim();
            complaint.Category = category;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Complaint {ComplaintId} updated by Student {StudentId}.", complaintId, studentId);

            return (true, null);
        }

        private async Task AddHistoryAsync(int complaintId, ComplaintStatus oldStatus, ComplaintStatus newStatus, string changedBy, string? remarks)
        {
            _context.ComplaintStatusHistories.Add(new ComplaintStatusHistory
            {
                ComplaintId = complaintId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = changedBy,
                Remarks = remarks
            });
            await _context.SaveChangesAsync();
        }
    }
}