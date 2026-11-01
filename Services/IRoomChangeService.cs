using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public interface IRoomChangeService
    {
        Task<(bool Success, string? ErrorMessage)> CreateRequestAsync(int studentId, int requestedRoomId, string reason);
        Task<List<RoomChangeRequest>> GetRequestsForStudentAsync(int studentId);
        Task<List<RoomChangeRequest>> GetPendingRequestsAsync(int? hostelId);
        Task<RoomChangeRequest?> GetRequestByIdAsync(int requestId);
        Task<(bool Success, string? ErrorMessage)> ApproveRequestAsync(int requestId);
        Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int requestId);
    }
}