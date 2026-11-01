using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hostel_hub.Models;
using Hostel_hub.Services;

namespace Hostel_hub.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Feedback")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IWardenContext _wardenContext;

        public FeedbackController(IFeedbackService feedbackService, IWardenContext wardenContext)
        {
            _feedbackService = feedbackService;
            _wardenContext = wardenContext;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(FeedbackSource? source)
        {
            var scopedHostelId = await _wardenContext.GetScopedHostelIdAsync(GetCurrentUserId());
            var feedback = await _feedbackService.GetAllFeedbackAsync(source, scopedHostelId);
            ViewBag.SelectedSource = source;
            return View(feedback);
        }

        private int GetCurrentUserId()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}