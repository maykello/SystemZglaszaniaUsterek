using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.ViewModels;

namespace SystemZglaszaniaUsterek.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(ILogger<TicketsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new TicketCreateViewModel
            {
                Categories = new List<CategoryModel>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(TicketCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return Ok(new { message = "Usterka będzie zgłoszona przez backend" });
        }
    }
}
