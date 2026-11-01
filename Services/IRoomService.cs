using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public interface IRoomService
    {
        Task<List<Room>> GetRoomsByHostelAsync(int? hostelId);
        Task<Room?> GetRoomByIdAsync(int roomId);
        Task<(bool Success, string? ErrorMessage)> CreateRoomAsync(int hostelId, string roomNumber, int capacity);
        Task<(bool Success, string? ErrorMessage)> UpdateRoomAsync(int roomId, string roomNumber, int capacity);
        Task<(bool Success, string? ErrorMessage)> DeleteRoomAsync(int roomId);
        Task<(bool Success, string? ErrorMessage)> AllocateStudentAsync(int studentId, int roomId);
    }
}