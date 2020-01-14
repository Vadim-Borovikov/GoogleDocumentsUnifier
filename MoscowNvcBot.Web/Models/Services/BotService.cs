using System.Collections.Generic;
using MoscowNvcBot.Web.Models.Commands;
using Telegram.Bot;

namespace MoscowNvcBot.Web.Models.Services
{
    internal class BotService : IBotService
    {
        public TelegramBotClient Client { get; }
        public IReadOnlyList<Command> Commands { get; }

        public BotService()
        {
            var commands = new List<Command>
            {
                new StartCommand()
            };
            Commands = commands.AsReadOnly();

            Client = new TelegramBotClient(Configuration.Token);
            Client.SetWebhookAsync(Configuration.Url).Wait();
        }
    }
}