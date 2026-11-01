using Microsoft.EntityFrameworkCore;
using Hostel_hub.Models;
using Hostel_hub.Services;

namespace Hostel_hub.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(ApplicationDbContext context, IAuthService authService, IConfiguration configuration)
        {
            bool adminExists = await context.Users.AnyAsync(u => u.Role == UserRole.Admin && !context.Wardens.Any(w => w.UserId == u.UserId));
            if (adminExists)
            {
                return;
            }

            string? adminEmail = configuration["SeedAdmin:Email"];
            string? adminPassword = configuration["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var admin = new User
            {
                Email = adminEmail.Trim().ToLowerInvariant(),
                PasswordHash = authService.HashPassword(adminPassword),
                Role = UserRole.Admin
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}