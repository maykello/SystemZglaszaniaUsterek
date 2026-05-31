using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.Enums;
using BCrypt.Net;

namespace SystemZglaszaniaUsterek.Data
{
    public static class DbSeeder
    {
        public static void SeedAll(SystemZglaszaniaUsterekDbContext context)
        {
            SeedUsers(context);
            SeedStatuses(context);
            SeedCategories(context);
            SeedPriorities(context);
        }

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

        public static void SeedStatuses(SystemZglaszaniaUsterekDbContext context)
        {
            var defaults = new (string Name, bool IsClosed)[]
            {
                ("Nowe", false),
                ("W trakcie", false),
                ("Zamknięte", true)
            };

            var changed = false;
            foreach (var (name, isClosed) in defaults)
            {
                var existing = context.Statuses.FirstOrDefault(s => s.Name == name);
                if (existing == null)
                {
                    context.Statuses.Add(new StatusModel { Name = name, IsClosed = isClosed });
                    changed = true;
                }
                else if (existing.IsClosed != isClosed)
                {
                    existing.IsClosed = isClosed;
                    changed = true;
                }
            }

            if (changed)
            {
                context.SaveChanges();
            }
        }

        public static void SeedCategories(SystemZglaszaniaUsterekDbContext context)
        {
            if (!context.Categories.Any(c => c.Name == "Inne"))
            {
                context.Categories.Add(new CategoryModel { Name = "Inne" });
                context.SaveChanges();
            }
        }

        public static void SeedPriorities(SystemZglaszaniaUsterekDbContext context)
        {
            var defaults = new[] { "Niski", "Średni", "Wysoki", "Krytyczny" };

            var changed = false;
            foreach (var name in defaults)
            {
                if (!context.Priorities.Any(p => p.Name == name))
                {
                    context.Priorities.Add(new PriorityModel { Name = name });
                    changed = true;
                }
            }

            if (changed)
            {
                context.SaveChanges();
            }
        }
    }
}
