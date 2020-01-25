using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MoscowNvcBot.Web.Models.Commands;
using MoscowNvcBot.Web.Models.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoscowNvcBot.Web.Controllers
{
    public class UpdateController : Controller
    {
        private readonly IBotService _botService;

        public UpdateController(IBotService botService) { _botService = botService; }

        [HttpPost]
        public async Task<OkResult> Post([FromBody]Update update)
        {
            if (update != null)
            {
                Command command;
                switch (update.Type)
                {
                    case UpdateType.Message:
                        Message message = update.Message;

                        command = _botService.Commands.FirstOrDefault(c => c.Contains(message));
                        if (command != null)
                        {
                            try
                            {
                                await command.ExecuteAsync(message, _botService.Client);
                            }
                            catch (Exception exception)
                            {
                                await command.HandleExceptionAsync(exception, message.Chat.Id, _botService.Client);
                            }
                        }
                        break;
                    case UpdateType.CallbackQuery:
                        CallbackQuery query = update.CallbackQuery;

                        command = _botService.Commands.FirstOrDefault(c => query.Data.Contains(c.Name));
                        if (command != null)
                        {
                            string queryData = query.Data.Replace(command.Name, "");
                            try
                            {
                                await command.InvokeAsync(query.Message, _botService.Client, queryData);
                            }
                            catch (Exception exception)
                            {
                                await
                                    command.HandleExceptionAsync(exception, query.Message.Chat.Id, _botService.Client);
                            }
                        }
                        break;
                }
            }

            return Ok();
        }
    }
}
