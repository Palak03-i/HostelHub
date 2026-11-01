using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(ApplicationDbContext context, ILogger<FeedbackService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Feedback>> GetFeedbackForStudentAsync(int studentId)
        {
            return await _context.Feedbacks
                .Include(f => f.Complaint)
                .Where(f => f.StudentId == studentId)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Feedback>> GetAllFeedbackAsync(FeedbackSource? source, int? scopedHostelId)
        {
            IQueryable<Feedback> query = _context.Feedbacks
                .Include(f => f.Student)
                .Include(f => f.Complaint);

            if (source.HasValue)
            {
                query = query.Where(f => f.Source == source.Value);
            }

            if (scopedHostelId.HasValue)
            {
                query = query.Where(f => f.Student!.HostelId == scopedHostelId.Value);
            }

            return await query.OrderByDescending(f => f.SubmittedAt).ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> SubmitFeedbackAsync(int studentId, FeedbackSource source, int? complaintId, int rating, string? comments)
        {
            if (!Enum.IsDefined(source) || rating < 1 || rating > 5 || comments?.Length > 500)
                return (false, "Choose a valid feedback type, a rating from 1 to 5, and at most 500 comment characters.");
            if (!await _context.Students.AnyAsync(s => s.StudentId == studentId))
                return (false, "Student not found.");

            if (source == FeedbackSource.Complaint)
            {
                if (complaintId == null)
                {
                    _logger.LogWarning("Failed to submit feedback: Missing complaintId for complaint feedback.");
                    return (false, "A complaint must be selected for complaint feedback.");
                }

                var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.ComplaintId == complaintId.Value);
                if (complaint == null || complaint.StudentId != studentId)
                {
                    _logger.LogWarning("Failed to submit feedback: Complaint {ComplaintId} not found or mismatch for Student {StudentId}.", complaintId, studentId);
                    return (false, "Complaint not found.");
                }

                if (complaint.Status != ComplaintStatus.Resolved)
                {
                    _logger.LogWarning("Failed to submit feedback: Complaint {ComplaintId} is not resolved.", complaintId);
                    return (false, "Feedback can only be submitted for resolved complaints.");
                }

                bool alreadySubmitted = await _context.Feedbacks
                    .AnyAsync(f => f.StudentId == studentId && f.ComplaintId == complaintId.Value);
                if (alreadySubmitted)
                {
                    _logger.LogWarning("Failed to submit feedback: Duplicate feedback by Student {StudentId} for Complaint {ComplaintId}.", studentId, complaintId);
                    return (false, "You have already submitted feedback for this complaint.");
                }
            }
            else
            {
                // Mess/General feedback carries no ComplaintId at all.
                complaintId = null;
            }

            _context.Feedbacks.Add(new Feedback
            {
                StudentId = studentId,
                Source = source,
                ComplaintId = complaintId,
                Rating = rating,
                Comments = comments
            });

            await _context.SaveChangesAsync();

            if (source == FeedbackSource.Complaint)
            {
                _logger.LogInformation("Complaint feedback submitted by Student {StudentId} for Complaint {ComplaintId} with rating {Rating}.", studentId, complaintId, rating);
            }
            else
            {
                _logger.LogInformation("Feedback submitted by Student {StudentId} for source {Source} with rating {Rating}.", studentId, source, rating);
            }

            return (true, null);
        }
    }
}