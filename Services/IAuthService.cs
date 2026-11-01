using System.Security.Claims;

namespace Hostel_hub.Services
{
    public interface IAuthService
    {
        string HashPassword(string plainPassword);
        bool VerifyPassword(string plainPassword, string storedHash);
        Task SignInAsync(HttpContext httpContext, int userId, string email, string role);
        Task SignOutAsync(HttpContext httpContext);
    }
}