using Hostel_hub.Models;
using Hostel_hub.ViewModels;
using Hostel_hub.Data;
using System;

namespace Hostel_hub.Services
{
    public class RoomService : IRoomService
    {
        public Task<List<Room>> GetRoomsByHostelAsync(int? hostelId) => throw new NotImplementedException();
        public Task<Room?> GetRoomByIdAsync(int roomId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> CreateRoomAsync(int hostelId, string roomNumber, int capacity) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> UpdateRoomAsync(int roomId, string roomNumber, int capacity) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> DeleteRoomAsync(int roomId) => throw new NotImplementedException();
        public Task<(bool Success, string? ErrorMessage)> AllocateStudentAsync(int studentId, int roomId) => throw new NotImplementedException();
    }
}
