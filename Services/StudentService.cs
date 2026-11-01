using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class StudentService : IStudentService
    {
        public Task<Student?> GetProfileByUserIdAsync(int userId) => throw new NotImplementedException();
        public Task<bool> UpdateProfileAsync( int userId, string fullName, string? phoneNumber) => throw new NotImplementedException();
        public Task<List<Student>> GetAllStudentsAsync( string? searchTerm, int? hostelId) => throw new NotImplementedException();
        public Task<Student?> GetStudentByIdAsync(int studentId) => throw new NotImplementedException();
        public Task<List<Student>> GetUnallocatedStudentsAsync(int hostelId) => throw new NotImplementedException();
        public Task<bool> AssignHostelAsync(int studentId, int hostelId) => throw new NotImplementedException();
    }
}
