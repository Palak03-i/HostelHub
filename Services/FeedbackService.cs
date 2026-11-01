using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class FeedbackService : IFeedbackService
    {
        public Task<List<Feedback>> GetFeedbackForStudentAsync(int studentId) => throw new NotImplementedException();
        public Task<List<Feedback>> GetAllFeedbackAsync(FeedbackSource? source, int? scopedHostelId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> SubmitFeedbackAsync(int studentId, FeedbackSource source, int? complaintId, int rating, string? comments) => throw new NotImplementedException();
    }
}
