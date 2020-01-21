﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MoscowNvcBot.Web.Models.Commands;
using MoscowNvcBot.Web.Models.Services;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        private readonly IBotService _botService;

        public HomeController(IBotService botService) { _botService = botService; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            User user = await _botService.Client.GetMeAsync();

            return View(user);
        }

        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            if (update != null)
            {
                Message message = update.Message;

                Command command = _botService.Commands.FirstOrDefault(c => c.Contains(message));
                if (command != null)
                {
                    await command.ExecuteAsync(message, _botService.Client);
                }
            }

            return Ok();
        }
    }
}
