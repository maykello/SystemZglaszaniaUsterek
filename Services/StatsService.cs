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
        private readonly ILogger<StatsService> _logger;

        public StatsService(SystemZglaszaniaUsterekDbContext db, ILogger<StatsService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<HomeIndexViewModel> GetHomeStatsAsync(CancellationToken ct = default)
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);

            try
            {
                var rows = await _db.Tickets
                    .AsNoTracking()
                    .Select(t => new
                    {
                        IsClosed = t.Status != null && t.Status.IsClosed,
                        t.CreatedAt,
                        t.UpdatedAt
                    })
                    .ToListAsync(ct);

                var currentIssues = rows.Count(r => !r.IsClosed);
                var closed = rows.Where(r => r.IsClosed).ToList();
                var totalResolved = closed.Count;
                var weekResolved = closed.Count(r => r.UpdatedAt != null && r.UpdatedAt >= weekAgo);

                var spans = closed
                    .Where(r => r.UpdatedAt != null)
                    .Select(r => Math.Max(0, (r.UpdatedAt!.Value - r.CreatedAt).TotalHours))
                    .ToList();

                var avgHours = spans.Count == 0 ? 0 : spans.Average();
                var avgText = $"{Math.Round(avgHours, 0)}h";

                var announcements = await _db.Announcements
                    .AsNoTracking()
                    .Where(a => a.IsActive)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToListAsync(ct);

                return new HomeIndexViewModel
                {
                    CurrentIssues = currentIssues,
                    TotalResolved = totalResolved,
                    WeekResolved = weekResolved,
                    AvgResolutionTimeText = avgText,
                    Announcements = announcements
                };
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return new HomeIndexViewModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nie udało się pobrać statystyk strony głównej.");
                return new HomeIndexViewModel();
            }
        }
    }
}
