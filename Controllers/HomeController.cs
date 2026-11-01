using Hostel_hub.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Hostel_hub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            // Public visitor:
            // Show the HostelHub marketing / landing page.
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return View();
            }

            // Student:
            // Home should take the student directly to their personal dashboard.
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Dashboard", "Student");
            }

            // Maintenance Staff:
            // Home should take them to their maintenance dashboard.
            if (User.IsInRole("MaintenanceStaff"))
            {
                return RedirectToAction("Dashboard", "Maintenance");
            }

            // Super Admin or Warden:
            // Both use the Admin dashboard controller.
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // Safety fallback for any unexpected authenticated role.
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Home/Error")]
        [Route("Home/Error/{statusCode:int}")]
        public IActionResult Error(int? statusCode = null)
        {
            var exceptionFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            var statusCodeFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>();

            int code = statusCode ?? (exceptionFeature != null ? 500 : HttpContext.Response.StatusCode);
            if (code < 400 || code > 599)
            {
                code = 500;
            }

            Response.StatusCode = code;

            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = code
            };

            switch (code)
            {
                case 404:
                    model.Title = "Page Not Found";
                    model.Message = "The page you are looking for might have been removed, had its name changed, or is temporarily unavailable.";
                    break;
                case 403:
                    model.Title = "Access Denied";
                    model.Message = "You do not have permission to access this page.";
                    break;
                case 400:
                    model.Title = "Bad Request";
                    model.Message = "The server could not understand the request due to invalid syntax.";
                    break;
                default:
                    model.Title = "Something went wrong";
                    model.Message = "An unexpected error occurred while processing your request. Please try again later.";
                    break;
            }

            if (exceptionFeature?.Error != null)
            {
                _logger.LogError(
                    exceptionFeature.Error,
                    "Unhandled exception occurred while processing request at path {Path}. Tracking ID: {RequestId}.",
                    exceptionFeature.Path,
                    model.RequestId);
            }
            else if (code == 404)
            {
                _logger.LogWarning(
                    "HTTP 404 Not Found at path {Path}. Tracking ID: {RequestId}.",
                    statusCodeFeature?.OriginalPath ?? HttpContext.Request.Path.Value,
                    model.RequestId);
            }
            else if (code == 403)
            {
                _logger.LogWarning(
                    "HTTP 403 Access Denied at path {Path}. Tracking ID: {RequestId}.",
                    statusCodeFeature?.OriginalPath ?? HttpContext.Request.Path.Value,
                    model.RequestId);
            }
            else if (code >= 500)
            {
                _logger.LogError(
                    "Server error {StatusCode} at path {Path}. Tracking ID: {RequestId}.",
                    code,
                    statusCodeFeature?.OriginalPath ?? HttpContext.Request.Path.Value,
                    model.RequestId);
            }

            return View("Error", model);
        }
    }
}
