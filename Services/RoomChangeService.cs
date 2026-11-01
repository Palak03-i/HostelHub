using Microsoft.EntityFrameworkCore;
using Hostel_hub.Data;
using Hostel_hub.Models;

namespace Hostel_hub.Services
{
    public class RoomChangeService : IRoomChangeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomChangeService> _logger;

        public RoomChangeService(ApplicationDbContext context, ILogger<RoomChangeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateRequestAsync(int studentId, int requestedRoomId, string reason)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
            {
                _logger.LogWarning("Failed to create room change request: Student {StudentId} not found.", studentId);
                return (false, "Student not found.");
            }

            if (!student.RoomId.HasValue)
            {
                _logger.LogWarning("Failed to create room change request: Student {StudentId} has no room.", studentId);
                return (false, "You must have an existing room before requesting a change.");
            }

            if (student.RoomId.Value == requestedRoomId)
            {
                _logger.LogWarning("Failed to create room change request: Student {StudentId} requested same room {RoomId}.", studentId, requestedRoomId);
                return (false, "You are already in this room.");
            }

            var requestedRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == requestedRoomId);
            if (requestedRoom == null)
            {
                _logger.LogWarning("Failed to create room change request: Requested Room {RoomId} not found.", requestedRoomId);
                return (false, "Requested room not found.");
            }

            if (requestedRoom.HostelId != student.HostelId)
                return (false, "You can only request a room in your own hostel.");
            if (requestedRoom.CurrentOccupancy >= requestedRoom.Capacity)
                return (false, "The requested room is full.");
            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
                return (false, "A reason of 1 to 500 characters is required.");
            reason = reason.Trim();

            bool hasPendingRequest = await _context.RoomChangeRequests
                .AnyAsync(r => r.StudentId == studentId && r.Status == RoomChangeStatus.Pending);
            if (hasPendingRequest)
            {
                _logger.LogWarning("Failed to create room change request: Student {StudentId} already has pending request.", studentId);
                return (false, "You already have a pending room change request.");
            }

            var request = new RoomChangeRequest
            {
                StudentId = studentId,
                CurrentRoomId = student.RoomId.Value,
                RequestedRoomId = requestedRoomId,
                Reason = reason,
                Status = RoomChangeStatus.Pending
            };

            _context.RoomChangeRequests.Add(request);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Room change request {RequestId} submitted by Student {StudentId}.", request.RoomChangeRequestId, studentId);

            return (true, null);
        }

        public async Task<List<RoomChangeRequest>> GetRequestsForStudentAsync(int studentId)
        {
            return await _context.RoomChangeRequests
                .Include(r => r.CurrentRoom)
                .Include(r => r.RequestedRoom)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<RoomChangeRequest>> GetPendingRequestsAsync(int? hostelId)
        {
            IQueryable<RoomChangeRequest> query = _context.RoomChangeRequests
                .Include(r => r.Student)
                .Include(r => r.CurrentRoom)
                .Include(r => r.RequestedRoom)
                .Where(r => r.Status == RoomChangeStatus.Pending);

            if (hostelId.HasValue)
            {
                query = query.Where(r => r.RequestedRoom!.HostelId == hostelId.Value);
            }

            return await query.OrderBy(r => r.RequestedAt).ToListAsync();
        }

        public async Task<RoomChangeRequest?> GetRequestByIdAsync(int requestId)
        {
            return await _context.RoomChangeRequests
                .Include(r => r.Student)
                .Include(r => r.CurrentRoom)
                .Include(r => r.RequestedRoom)
                .FirstOrDefaultAsync(r => r.RoomChangeRequestId == requestId);
        }

        public async Task<(bool Success, string? ErrorMessage)> ApproveRequestAsync(int requestId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var request = await _context.RoomChangeRequests
                    .FirstOrDefaultAsync(r => r.RoomChangeRequestId == requestId);

                if (request == null)
                {
                    _logger.LogWarning("Failed to approve room change: Request {RequestId} not found.", requestId);
                    return (false, "Request not found.");
                }

                if (request.Status != RoomChangeStatus.Pending)
                {
                    _logger.LogWarning("Failed to approve room change: Request {RequestId} status is already {Status}.", requestId, request.Status);
                    return (false, $"This request has already been {request.Status}. It cannot be approved again.");
                }

                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == request.StudentId);
                var oldRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == request.CurrentRoomId);
                var newRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == request.RequestedRoomId);

                if (student == null || oldRoom == null || newRoom == null)
                {
                    _logger.LogWarning("Failed to approve room change {RequestId}: Associated entity missing.", requestId);
                    return (false, "Related student or room data could not be found.");
                }

                if (student.RoomId != oldRoom.RoomId || oldRoom.CurrentOccupancy < 1)
                    return (false, "The student allocation has changed. Reject this stale request.");
                if (newRoom.HostelId != student.HostelId || oldRoom.HostelId != student.HostelId)
                    return (false, "Room changes must stay within the student hostel.");

                // CRITICAL: re-check capacity NOW, at approval time — not using
                // any value from when the request was originally created.
                if (newRoom.CurrentOccupancy >= newRoom.Capacity)
                {
                    _logger.LogWarning("Failed to approve room change {RequestId}: Room {RoomNumber} is full.", requestId, newRoom.RoomNumber);
                    return (false, $"Cannot approve — room '{newRoom.RoomNumber}' is now full ({newRoom.CurrentOccupancy}/{newRoom.Capacity}).");
                }

                oldRoom.CurrentOccupancy -= 1;
                newRoom.CurrentOccupancy += 1;
                student.RoomId = newRoom.RoomId;
                student.HostelId = newRoom.HostelId;
                request.Status = RoomChangeStatus.Approved;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Room change request {RequestId} approved for Student {StudentId}.", requestId, request.StudentId);

                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("Concurrency conflict while approving RoomChangeRequest {RequestId}.", requestId);
                return (false, "One of the rooms was just modified by someone else. Please refresh and try again.");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int requestId)
        {
            var request = await _context.RoomChangeRequests.FirstOrDefaultAsync(r => r.RoomChangeRequestId == requestId);
            if (request == null)
            {
                _logger.LogWarning("Failed to reject room change: Request {RequestId} not found.", requestId);
                return (false, "Request not found.");
            }

            if (request.Status != RoomChangeStatus.Pending)
            {
                _logger.LogWarning("Failed to reject room change: Request {RequestId} status is already {Status}.", requestId, request.Status);
                return (false, $"This request has already been {request.Status}.");
            }

            request.Status = RoomChangeStatus.Rejected;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Room change request {RequestId} rejected for Student {StudentId}.", requestId, request.StudentId);

            return (true, null);
        }
    }
}