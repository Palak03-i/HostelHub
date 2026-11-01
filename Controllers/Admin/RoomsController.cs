using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hostel_hub.Services;
using Hostel_hub.ViewModels;

namespace Hostel_hub.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Rooms")]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IHostelService _hostelService;
        private readonly IWardenContext _wardenContext;

        private readonly IStudentService _studentService;

        public RoomsController(IRoomService roomService, IHostelService hostelService, IWardenContext wardenContext, IStudentService studentService)
        {
            _roomService = roomService;
            _hostelService = hostelService;
            _wardenContext = wardenContext;
            _studentService = studentService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            var rooms = await _roomService.GetRoomsByHostelAsync(scopedHostelId);
            return View(rooms);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            if (!await CanAccessHostelAsync(room.HostelId))
            {
                return Forbid();
            }

            return View(room);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            if (!scopedHostelId.HasValue)
            {
                TempData["ErrorMessage"] = "Only a Warden can create rooms for their own hostel.";
                return RedirectToAction("Index");
            }

            await PopulateHostelDropdownAsync();
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomViewModel model)
        {
            if (!await CanManageAsync(model.HostelId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await PopulateHostelDropdownAsync();
                return View(model);
            }

            var (success, errorMessage) = await _roomService.CreateRoomAsync(model.HostelId, model.RoomNumber, model.Capacity);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage!);
                await PopulateHostelDropdownAsync();
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(room.HostelId))
            {
                return Forbid();
            }

            var model = new RoomViewModel
            {
                HostelId = room.HostelId,
                RoomNumber = room.RoomNumber,
                Capacity = room.Capacity
            };

            return View(model);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomViewModel model)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(room.HostelId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage) = await _roomService.UpdateRoomAsync(id, model.RoomNumber, model.Capacity);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage!);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(room.HostelId))
            {
                return Forbid();
            }

            var (success, errorMessage) = await _roomService.DeleteRoomAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("Index");
        }

        private async Task PopulateHostelDropdownAsync()
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            var hostels = await _hostelService.GetAllHostelsAsync();

            if (scopedHostelId.HasValue)
            {
                hostels = hostels.Where(h => h.HostelId == scopedHostelId.Value).ToList();
            }

            ViewBag.Hostels = hostels;
        }

        private async Task<bool> CanAccessHostelAsync(int hostelId)
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            return !scopedHostelId.HasValue || scopedHostelId.Value == hostelId;
        }

        private async Task<bool> CanManageAsync(int hostelId)
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            return scopedHostelId.HasValue && scopedHostelId.Value == hostelId;
        }

        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
        [HttpGet("Allocate/{roomId}")]
        public async Task<IActionResult> Allocate(int roomId)
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(room.HostelId))
            {
                return Forbid();
            }


            ViewBag.Room = room;
            ViewBag.UnallocatedStudents =await _studentService.GetUnallocatedStudentsAsync(room.HostelId);
            return View();
        }

        [HttpPost("Allocate/{roomId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Allocate(int roomId, int studentId)
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            if (!await CanManageAsync(room.HostelId))
            {
                return Forbid();
            }
            var student = await _studentService.GetStudentByIdAsync(studentId);

            if (student == null)
            {
                return NotFound();
            }

            if (student.HostelId != room.HostelId)
            {
                return Forbid();
            }

            var (success, errorMessage) = await _roomService.AllocateStudentAsync(studentId, roomId);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("Details", new { id = roomId });
        }
    }
}