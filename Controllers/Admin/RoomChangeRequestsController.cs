using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hostel_hub.Services;

namespace Hostel_hub.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/RoomChangeRequests")]
    public class RoomChangeRequestsController : Controller
    {
        private readonly IRoomChangeService _roomChangeService;
        private readonly IWardenContext _wardenContext;
        private readonly ILogger<RoomChangeRequestsController> _logger;

        public RoomChangeRequestsController(IRoomChangeService roomChangeService, IWardenContext wardenContext, ILogger<RoomChangeRequestsController> logger)
        {
            _roomChangeService = roomChangeService;
            _wardenContext = wardenContext;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            var requests = await _roomChangeService.GetPendingRequestsAsync(scopedHostelId);
            return View(requests);
        }

        [HttpPost("Approve/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var adminId = GetCurrentUserId();
            if (!await CanManageRequestAsync(id))
            {
                _logger.LogWarning("Admin {AdminId} unauthorized to approve RoomChangeRequest {RequestId}.", adminId, id);
                return Forbid();
            }

            var (success, errorMessage) = await _roomChangeService.ApproveRequestAsync(id);
            if (!success)
            {
                _logger.LogWarning("Admin {AdminId} failed to approve RoomChangeRequest {RequestId}: {ErrorMessage}", adminId, id, errorMessage);
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                _logger.LogInformation("Room change request {RequestId} approved by Admin {AdminId}.", id, adminId);
                TempData["SuccessMessage"] = "Room change approved.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost("Reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var adminId = GetCurrentUserId();
            if (!await CanManageRequestAsync(id))
            {
                _logger.LogWarning("Admin {AdminId} unauthorized to reject RoomChangeRequest {RequestId}.", adminId, id);
                return Forbid();
            }

            var (success, errorMessage) = await _roomChangeService.RejectRequestAsync(id);
            if (!success)
            {
                _logger.LogWarning("Admin {AdminId} failed to reject RoomChangeRequest {RequestId}: {ErrorMessage}", adminId, id, errorMessage);
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                _logger.LogInformation("Room change request {RequestId} rejected by Admin {AdminId}.", id, adminId);
                TempData["SuccessMessage"] = "Room change rejected.";
            }

            return RedirectToAction("Index");
        }

        private async Task<bool> CanManageRequestAsync(int requestId)
        {
            var request = await _roomChangeService.GetRequestByIdAsync(requestId);
            if (request == null || request.RequestedRoom == null)
            {
                return false;
            }

            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            return scopedHostelId.HasValue && scopedHostelId.Value == request.RequestedRoom.HostelId;
        }

        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}