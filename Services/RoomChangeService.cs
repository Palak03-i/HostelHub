using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class RoomChangeService : IRoomChangeService
    {
        public Task<(bool Success, string? ErrorMessage)> CreateRequestAsync(int studentId, int requestedRoomId, string reason) => throw new NotImplementedException();
        public Task<List<RoomChangeRequest>> GetRequestsForStudentAsync(int studentId) => throw new NotImplementedException();
        public Task<List<RoomChangeRequest>> GetPendingRequestsAsync(int? hostelId) => throw new NotImplementedException();
        public Task<RoomChangeRequest?> GetRequestByIdAsync(int requestId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> ApproveRequestAsync(int requestId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int requestId) => throw new NotImplementedException();
    }
}
