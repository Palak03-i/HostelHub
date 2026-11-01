using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public interface IFeedbackService
    {
        Task<List<Feedback>> GetFeedbackForStudentAsync(int studentId);
        Task<List<Feedback>> GetAllFeedbackAsync(FeedbackSource? source, int? scopedHostelId);
        Task<(bool Success, string? ErrorMessage)> SubmitFeedbackAsync(int studentId, FeedbackSource source, int? complaintId, int rating, string? comments);
    }
}