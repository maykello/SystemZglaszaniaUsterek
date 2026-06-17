using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using SystemZglaszaniaUsterek.Models;
using SystemZglaszaniaUsterek.Services;

namespace SystemZglaszaniaUsterek.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStatsService _statsService;

        public HomeController(IStatsService statsService)
        {
            _statsService = statsService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var vm = await _statsService.GetHomeStatsAsync(ct);
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
