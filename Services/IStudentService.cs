using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public interface IStudentService
    {
        Task<Student?> GetProfileByUserIdAsync(int userId);

        Task<bool> UpdateProfileAsync(
            int userId,
            string fullName,
            string? phoneNumber);

        Task<List<Student>> GetAllStudentsAsync(
            string? searchTerm,
            int? hostelId);

        Task<Student?> GetStudentByIdAsync(int studentId);

        Task<List<Student>> GetUnallocatedStudentsAsync(int hostelId);

        Task<bool> AssignHostelAsync(int studentId, int hostelId);
    }
}