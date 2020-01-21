using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MoscowNvcBot.Web.Models;
using MoscowNvcBot.Web.Models.Services;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IBotService _botService;

        public HomeController(IBotService botService) { _botService = botService; }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            User model = await _botService.Client.GetMeAsync();
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            return View(model);
        }
    }
}
