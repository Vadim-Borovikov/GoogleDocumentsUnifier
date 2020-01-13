using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MoscowNvcBot.Web.Models;
using MoscowNvcBot.Web.Models.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Controllers
{
    [Route(AppSettings.Route)]
    public class UpdateController : Controller
    {
        // GET api/values
        [HttpGet]
        public string Get() => "Method GET unuvalable";

        // POST api/values
        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            if (update != null)
            {
                Message message = update.Message;
                TelegramBotClient client = await Bot.GetBotClientAsync();

                Command command = Bot.Commands.FirstOrDefault(c => c.Contains(message));
                if (command != null)
                {
                    await command.Execute(message, client);
                }
            }

            return Ok();
        }
    }
}
