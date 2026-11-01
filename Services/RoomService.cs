using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomService> _logger;

        public RoomService(ApplicationDbContext context, ILogger<RoomService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Room>> GetRoomsByHostelAsync(int? hostelId)
        {
            IQueryable<Room> query = _context.Rooms.Include(r => r.Hostel);

            if (hostelId.HasValue)
            {
                query = query.Where(r => r.HostelId == hostelId.Value);
            }

            return await query.OrderBy(r => r.Hostel!.Name).ThenBy(r => r.RoomNumber).ToListAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(int roomId)
        {
            return await _context.Rooms
                .Include(r => r.Hostel)
                .Include(r => r.Students)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateRoomAsync(int hostelId, string roomNumber, int capacity)
        {
            roomNumber = roomNumber.Trim();
            if (string.IsNullOrWhiteSpace(roomNumber) || roomNumber.Length > 20)
                return (false, "Room number must contain 1 to 20 characters.");

            if (capacity < 1 || capacity > 20)
            {
                return (false, "Capacity must be between 1 and 20.");
            }

            bool hostelExists = await _context.Hostels.AnyAsync(h => h.HostelId == hostelId);
            if (!hostelExists)
            {
                return (false, "Selected hostel does not exist.");
            }

            bool duplicateRoomNumber = await _context.Rooms
                .AnyAsync(r => r.HostelId == hostelId && r.RoomNumber == roomNumber);
            if (duplicateRoomNumber)
            {
                _logger.LogWarning("Failed to create room: RoomNumber {RoomNumber} already exists in Hostel {HostelId}.", roomNumber, hostelId);
                return (false, $"Room number '{roomNumber}' already exists in this hostel.");
            }

            var room = new Room
            {
                HostelId = hostelId,
                RoomNumber = roomNumber,
                Capacity = capacity,
                CurrentOccupancy = 0
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Room {RoomNumber} created in Hostel {HostelId} with capacity {Capacity}.", roomNumber, hostelId, capacity);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateRoomAsync(int roomId, string roomNumber, int capacity)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (room == null)
            {
                _logger.LogWarning("Failed to update room: Room {RoomId} not found.", roomId);
                return (false, "Room not found.");
            }

            roomNumber = roomNumber.Trim();
            if (string.IsNullOrWhiteSpace(roomNumber) || roomNumber.Length > 20)
                return (false, "Room number must contain 1 to 20 characters.");

            if (capacity < 1 || capacity > 20)
            {
                return (false, "Capacity must be between 1 and 20.");
            }

            if (capacity < room.CurrentOccupancy)
            {
                return (false, $"Cannot set capacity below current occupancy ({room.CurrentOccupancy}).");
            }

            bool duplicateRoomNumber = await _context.Rooms
                .AnyAsync(r => r.HostelId == room.HostelId && r.RoomNumber == roomNumber && r.RoomId != roomId);
            if (duplicateRoomNumber)
            {
                _logger.LogWarning("Failed to update room: RoomNumber {RoomNumber} already exists in Hostel {HostelId}.", roomNumber, room.HostelId);
                return (false, $"Room number '{roomNumber}' already exists in this hostel.");
            }

            room.RoomNumber = roomNumber;
            room.Capacity = capacity;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Room {RoomId} updated: RoomNumber {RoomNumber}, Capacity {Capacity}.", roomId, roomNumber, capacity);

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteRoomAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Students)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
            {
                _logger.LogWarning("Failed to delete room: Room {RoomId} not found.", roomId);
                return (false, "Room not found.");
            }

            if (room.Students.Any())
            {
                _logger.LogWarning("Failed to delete Room {RoomId}: Room has active students.", roomId);
                return (false, $"Cannot delete room '{room.RoomNumber}' — it has {room.Students.Count} student(s) currently allocated.");
            }

            if (await _context.RoomChangeRequests.AnyAsync(r => r.CurrentRoomId == roomId || r.RequestedRoomId == roomId))
                return (false, "This room has room-change history and cannot be deleted.");

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Room {RoomId} deleted.", roomId);

            return (true, null);
        }
        public async Task<(bool Success, string? ErrorMessage)> AllocateStudentAsync(int studentId, int roomId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
                if (student == null)
                {
                    _logger.LogWarning("Failed to allocate student: Student {StudentId} not found.", studentId);
                    return (false, "Student not found.");
                }

                if (student.RoomId.HasValue)
                {
                    _logger.LogWarning("Failed to allocate student: Student {StudentId} already has Room {RoomId}.", studentId, student.RoomId.Value);
                    return (false, "This student is already allocated to a room. Use Room Change instead.");
                }

                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId);
                if (room == null)
                {
                    _logger.LogWarning("Failed to allocate student: Room {RoomId} not found.", roomId);
                    return (false, "Room not found.");
                }

                if (student.HostelId != room.HostelId)
                    return (false, "Student and room must belong to the same hostel.");

                if (room.CurrentOccupancy >= room.Capacity)
                {
                    _logger.LogWarning("Failed to allocate Student {StudentId} to Room {RoomId}: Room is full.", studentId, roomId);
                    return (false, $"Room '{room.RoomNumber}' is full ({room.CurrentOccupancy}/{room.Capacity}).");
                }

                room.CurrentOccupancy += 1;
                student.RoomId = room.RoomId;
                student.HostelId = room.HostelId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Student {StudentId} was assigned to Room {RoomId}.", studentId, roomId);

                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("Concurrency conflict while allocating Student {StudentId} to Room {RoomId}.", studentId, roomId);
                return (false, "This room was just modified by someone else. Please refresh and try again.");
            }
        }
    }
}