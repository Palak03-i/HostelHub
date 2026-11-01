using System.Security.Claims;
using Hostel_hub.Data;
using Hostel_hub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Hostel_hub.Security;

// Recheck account state on every MVC request, including existing cookie sessions.
public sealed class AccountAccessFilter(ApplicationDbContext db) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var principal = context.HttpContext.User;
        if (principal.Identity?.IsAuthenticated != true) return;
        if (context.RouteData.Values["controller"]?.ToString() == "Account" &&
            context.RouteData.Values["action"]?.ToString() is "Logout" or "AccessDenied") return;
        if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        { context.Result = new ForbidResult(); return; }
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null || !principal.IsInRole(user.Role.ToString()))
        { context.Result = new ForbidResult(); return; }
        // Permit logout and access-denied pages even after an account is disabled.
        if (context.RouteData.Values["controller"]?.ToString() == "Account") return;
        if (user.Role == UserRole.MaintenanceStaff &&
            !await db.MaintenanceStaff.AnyAsync(s => s.UserId == id && s.IsActive))
            context.Result = new ForbidResult();
        if (user.Role == UserRole.Admin &&
            await db.Wardens.AnyAsync(w => w.UserId == id && w.HostelId == null))
            context.Result = new ForbidResult();
    }
}
