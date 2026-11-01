using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using static Hostel_hub.Models.Complaint;

namespace Hostel_hub.Services
{
    public interface IComplaintService
    {
        Task<(bool Success, string? ErrorMessage)> CreateComplaintAsync(int studentId, string title, string description, ComplaintCategory category);
        Task<List<Complaint>> GetComplaintsForStudentAsync(int studentId);
        Task<List<Complaint>> GetComplaintsForStudentAsync(int studentId, ComplaintFilterViewModel filter);
        Task<List<Complaint>> GetAllComplaintsAsync(int? hostelId, ComplaintStatus? status, ComplaintCategory? category, ComplaintPriority? priority);
        Task<List<Complaint>> GetAllComplaintsAsync(int? hostelId, ComplaintFilterViewModel filter);
        Task<List<Complaint>> GetAssignedComplaintsAsync(int staffId);
        Task<List<Complaint>> GetAssignedComplaintsAsync(int staffId, ComplaintFilterViewModel filter);
        Task<Complaint?> GetComplaintByIdAsync(int complaintId);
        Task<List<ComplaintStatusHistory>> GetHistoryAsync(int complaintId);
        Task<(bool Success, string? ErrorMessage)> SetPriorityAsync(int complaintId, ComplaintPriority priority, string changedBy);
        Task<(bool Success, string? ErrorMessage)> AssignStaffAsync(int complaintId, int staffId, string changedBy);
        Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(int complaintId, ComplaintStatus newStatus, string changedBy, string? remarks);
        Task<(bool Success, string? ErrorMessage)> UpdateComplaintAsync(int complaintId, int studentId, string title, string description, ComplaintCategory category);
    }
}