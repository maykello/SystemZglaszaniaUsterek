using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemZglaszaniaUsterek.Models.ViewModels;
using SystemZglaszaniaUsterek.Services;

namespace SystemZglaszaniaUsterek.Controllers
{
    [Authorize(Roles = "Technician,Administrator")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reports;

        public ReportsController(IReportService reports)
        {
            _reports = reports;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ReportFilterViewModel? filter, CancellationToken ct)
        {
            filter ??= new ReportFilterViewModel();
            var vm = await _reports.GetRepairReportAsync(filter, ct);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterViewModel? filter, CancellationToken ct)
        {
            filter ??= new ReportFilterViewModel();
            var bytes = await _reports.ExportRepairReportCsvAsync(filter, ct);
            var fileName = $"raport-napraw-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
    }
}
