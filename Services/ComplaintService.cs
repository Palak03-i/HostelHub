using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using static Hostel_hub.Models.Complaint;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class ComplaintService : IComplaintService
    {
        public Task<(bool Success, string? ErrorMessage)> CreateComplaintAsync(int studentId, string title, string description, ComplaintCategory category) => throw new NotImplementedException();
        public Task<List<Complaint>> GetComplaintsForStudentAsync(int studentId) => throw new NotImplementedException();
        public Task<List<Complaint>> GetComplaintsForStudentAsync(int studentId, ComplaintFilterViewModel filter) => throw new NotImplementedException();
        public Task<List<Complaint>> GetAllComplaintsAsync(int? hostelId, ComplaintStatus? status, ComplaintCategory? category, ComplaintPriority? priority) => throw new NotImplementedException();
        public Task<List<Complaint>> GetAllComplaintsAsync(int? hostelId, ComplaintFilterViewModel filter) => throw new NotImplementedException();
        public Task<List<Complaint>> GetAssignedComplaintsAsync(int staffId) => throw new NotImplementedException();
        public Task<List<Complaint>> GetAssignedComplaintsAsync(int staffId, ComplaintFilterViewModel filter) => throw new NotImplementedException();
        public Task<Complaint?> GetComplaintByIdAsync(int complaintId) => throw new NotImplementedException();
        public Task<List<ComplaintStatusHistory>> GetHistoryAsync(int complaintId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> SetPriorityAsync(int complaintId, ComplaintPriority priority, string changedBy) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> AssignStaffAsync(int complaintId, int staffId, string changedBy) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(int complaintId, ComplaintStatus newStatus, string changedBy, string? remarks) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> UpdateComplaintAsync(int complaintId, int studentId, string title, string description, ComplaintCategory category) => throw new NotImplementedException();
    }
}
