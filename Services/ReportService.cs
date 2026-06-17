using Mapster;
using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.Enums;
using SystemZglaszaniaUsterek.Models.ViewModels;
using System.Globalization;
using System.Text;

namespace SystemZglaszaniaUsterek.Services
{
    public interface IReportService
    {
        Task<ReportsRepairViewModel> GetRepairReportAsync(ReportFilterViewModel filter, CancellationToken ct = default);
        Task<byte[]> ExportRepairReportCsvAsync(ReportFilterViewModel filter, CancellationToken ct = default);
    }

    public class ReportService : IReportService
    {
        private readonly SystemZglaszaniaUsterekDbContext _db;

        public ReportService(SystemZglaszaniaUsterekDbContext db)
        {
            _db = db;
        }

        public async Task<ReportsRepairViewModel> GetRepairReportAsync(ReportFilterViewModel filter, CancellationToken ct = default)
        {
            try
            {
                var query = BuildQuery(filter);

                var total = await query.CountAsync(ct);
                var open = await query.CountAsync(t => t.Status != null && !t.Status.IsClosed, ct);
                var closed = await query.CountAsync(t => t.Status != null && t.Status.IsClosed, ct);

                var resolved = await query
                    .Where(t => t.Status != null && t.Status.IsClosed && t.UpdatedAt != null)
                    .Select(t => new { t.CreatedAt, t.UpdatedAt })
                    .ToListAsync(ct);

                var avgHours = resolved.Count == 0
                    ? 0
                    : resolved.Select(x => Math.Max(0, ((x.UpdatedAt!.Value) - x.CreatedAt).TotalHours)).Average();

                var byStatus = await query
                    .Where(t => t.Status != null)
                    .GroupBy(t => t.Status!.Name)
                    .Select(g => new ReportRowViewModel { Label = g.Key, Count = g.Count() })
                    .OrderByDescending(r => r.Count)
                    .ToListAsync(ct);

                var byPriority = await query
                    .Where(t => t.Priority != null)
                    .GroupBy(t => t.Priority!.Name)
                    .Select(g => new ReportRowViewModel { Label = g.Key, Count = g.Count() })
                    .OrderByDescending(r => r.Count)
                    .ToListAsync(ct);

                var byCategory = await query
                    .GroupBy(t => t.Category != null ? t.Category.Name : "(brak)")
                    .Select(g => new ReportRowViewModel { Label = g.Key, Count = g.Count() })
                    .OrderByDescending(r => r.Count)
                    .ToListAsync(ct);

                var byTechnician = await query
                    .GroupBy(t => t.Technician != null ? t.Technician.Username : "(nieprzydzielony)")
                    .Select(g => new ReportRowViewModel { Label = g.Key, Count = g.Count() })
                    .OrderByDescending(r => r.Count)
                    .ToListAsync(ct);

                var page = filter.Page < 1 ? 1 : filter.Page;
                var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);

                var items = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ProjectToType<TicketListItemViewModel>()
                    .ToListAsync(ct);

                var technicians = await _db.Users.AsNoTracking()
                    .Where(u => !u.IsDeleted && (u.Role == Role.Technician || u.Role == Role.Administrator))
                    .OrderBy(u => u.Username)
                    .ToListAsync(ct);

                var categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

                filter.Page = page;
                filter.PageSize = pageSize;

                return new ReportsRepairViewModel
                {
                    Filter = filter,
                    Total = total,
                    Open = open,
                    Closed = closed,
                    AvgResolutionHours = Math.Round(avgHours, 1),
                    ByStatus = byStatus,
                    ByPriority = byPriority,
                    ByCategory = byCategory,
                    ByTechnician = byTechnician,
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    Technicians = technicians,
                    Categories = categories
                };
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return new ReportsRepairViewModel { Filter = filter };
            }
        }

        public async Task<byte[]> ExportRepairReportCsvAsync(ReportFilterViewModel filter, CancellationToken ct = default)
        {
            try
            {
                var query = BuildQuery(filter);

                var rows = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        Status = t.Status != null ? t.Status.Name : "",
                        Priority = t.Priority != null ? t.Priority.Name : "",
                        Category = t.Category != null ? t.Category.Name : "",
                        Reporter = t.Reporter != null ? t.Reporter.Username : "",
                        Technician = t.Technician != null ? t.Technician.Username : "",
                        t.CreatedAt,
                        t.UpdatedAt
                    })
                    .ToListAsync(ct);

                var sb = new StringBuilder();
                sb.AppendLine("Id;Tytul;Status;Priorytet;Kategoria;Zglaszajacy;Technik;Utworzono;Zaktualizowano");

                var ic = CultureInfo.InvariantCulture;
                foreach (var r in rows)
                {
                    sb.Append(r.Id).Append(';')
                      .Append(Csv(r.Title)).Append(';')
                      .Append(Csv(r.Status)).Append(';')
                      .Append(Csv(r.Priority)).Append(';')
                      .Append(Csv(r.Category)).Append(';')
                      .Append(Csv(r.Reporter)).Append(';')
                      .Append(Csv(r.Technician)).Append(';')
                      .Append(r.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", ic)).Append(';')
                      .Append(r.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss", ic) ?? "")
                      .AppendLine();
                }

                // UTF-8 BOM so Excel reads diacritics
                var bom = new byte[] { 0xEF, 0xBB, 0xBF };
                var content = Encoding.UTF8.GetBytes(sb.ToString());
                var output = new byte[bom.Length + content.Length];
                Buffer.BlockCopy(bom, 0, output, 0, bom.Length);
                Buffer.BlockCopy(content, 0, output, bom.Length, content.Length);
                return output;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Array.Empty<byte>();
            }
        }

        private IQueryable<TicketModel> BuildQuery(ReportFilterViewModel filter)
        {
            IQueryable<TicketModel> query = _db.Tickets.AsNoTracking()
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Category)
                .Include(t => t.Reporter)
                .Include(t => t.Technician);

            if (filter.DateFrom.HasValue)
            {
                var from = DateTime.SpecifyKind(filter.DateFrom.Value.Date, DateTimeKind.Utc);
                query = query.Where(t => t.CreatedAt >= from);
            }
            if (filter.DateTo.HasValue)
            {
                var to = DateTime.SpecifyKind(filter.DateTo.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(t => t.CreatedAt < to);
            }
            if (filter.TechnicianId.HasValue)
            {
                query = query.Where(t => t.Technician != null && t.Technician.Id == filter.TechnicianId.Value);
            }
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
            }

            return query;
        }

        private static string Csv(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            var needsQuotes = value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (!needsQuotes)
            {
                return value;
            }
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
