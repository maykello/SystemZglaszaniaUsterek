using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.Enums;
using BCrypt.Net;

namespace SystemZglaszaniaUsterek.Data
{
    public static class DbSeeder
    {
        public static void SeedUsers(SystemZglaszaniaUsterekDbContext context)
        {
            if (!context.Users.Any())
            {
                var adminUser = new UserModel
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                    Role = Role.Administrator,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                context.SaveChanges();
            }
        }
    }
}
