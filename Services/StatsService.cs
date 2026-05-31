using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.ViewModels;

namespace SystemZglaszaniaUsterek.Services
{
    public interface IStatsService
    {
        Task<HomeIndexViewModel> GetHomeStatsAsync(CancellationToken ct = default);
    }

    public class StatsService : IStatsService
    {
        private readonly SystemZglaszaniaUsterekDbContext _db;

        public StatsService(SystemZglaszaniaUsterekDbContext db)
        {
            _db = db;
        }

        public async Task<HomeIndexViewModel> GetHomeStatsAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var weekAgo = now.AddDays(-7);

            var ticketsWithStatus = _db.Tickets
                .AsNoTracking()
                .Include(t => t.Status);

            var currentIssues = await ticketsWithStatus
                .CountAsync(t => t.Status != null && !t.Status.IsClosed, ct);

            var closedTickets = ticketsWithStatus
                .Where(t => t.Status != null && t.Status.IsClosed);

            var totalResolved = await closedTickets.CountAsync(ct);

            var weekResolved = await closedTickets
                .CountAsync(t => t.UpdatedAt != null && t.UpdatedAt >= weekAgo, ct);

            var resolvedSpans = await closedTickets
                .Where(t => t.UpdatedAt != null)
                .Select(t => new { t.CreatedAt, t.UpdatedAt })
                .ToListAsync(ct);

            string avgText = "0h";
            if (resolvedSpans.Count > 0)
            {
                var avgHours = resolvedSpans
                    .Select(x => Math.Max(0, ((x.UpdatedAt!.Value) - x.CreatedAt).TotalHours))
                    .DefaultIfEmpty(0)
                    .Average();

                avgText = $"{Math.Round(avgHours, 0)}h";
            }

            return new HomeIndexViewModel
            {
                CurrentIssues = currentIssues,
                TotalResolved = totalResolved,
                WeekResolved = weekResolved,
                AvgResolutionTimeText = avgText
            };
        }
    }
}
