using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentService> _logger;

        public StudentService(ApplicationDbContext context, ILogger<StudentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Student?> GetProfileByUserIdAsync(int userId)
        {
            return await _context.Students
                .Include(s => s.Hostel)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<bool> UpdateProfileAsync(int userId, string fullName, string? phoneNumber)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                _logger.LogWarning("Failed to update profile: Student with UserId {UserId} not found.", userId);
                return false;
            }

            student.FullName = fullName;
            student.PhoneNumber = phoneNumber;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Profile updated for Student {StudentId} (UserId {UserId}).", student.StudentId, userId);
            return true;
        }
        public async Task<List<Student>> GetAllStudentsAsync(string? searchTerm, int? hostelId)
        {
            IQueryable<Student> query = _context.Students
                .Include(s => s.Hostel)
                .Include(s => s.Room);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s =>
                    s.FullName.Contains(searchTerm) ||
                    s.RollNumber.Contains(searchTerm));
            }

            if (hostelId.HasValue)
            {
                query = query.Where(s => s.HostelId == hostelId.Value);
            }

            return await query.OrderBy(s => s.FullName).ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.Hostel)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }
        public async Task<List<Student>> GetUnallocatedStudentsAsync(int hostelId)
        {
            return await _context.Students
                .Where(s =>
                    s.RoomId == null &&
                    s.HostelId == hostelId)
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }
        public async Task<bool> AssignHostelAsync(
    int studentId,
    int hostelId)
        {
            var student =
                await _context.Students
                    .FirstOrDefaultAsync(s =>
                        s.StudentId == studentId);

            if (student == null)
            {
                return false;
            }

            // Initial hostel assignment only.
            // Do not use this method to transfer an already assigned student.
            if (student.HostelId.HasValue)
            {
                return false;
            }

            // Student should not already have a room before hostel assignment.
            if (student.RoomId.HasValue)
            {
                return false;
            }

            bool hostelExists =
                await _context.Hostels
                    .AnyAsync(h =>
                        h.HostelId == hostelId);

            if (!hostelExists)
            {
                return false;
            }

            student.HostelId = hostelId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Student {StudentId} was assigned to Hostel {HostelId}.", studentId, hostelId);

            return true;
        }

    }
}