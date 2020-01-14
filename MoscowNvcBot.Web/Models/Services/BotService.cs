using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MoscowNvcBot.Web.Models.Commands;
using Telegram.Bot;

namespace MoscowNvcBot.Web.Models.Services
{
    internal class BotService : IBotService
    {
        public TelegramBotClient Client { get; }
        public IReadOnlyList<Command> Commands { get; }

        public BotService(IOptions<BotConfiguration> options)
        {
            BotConfiguration config = options.Value;

            Client = new TelegramBotClient(config.Token);
            Client.SetWebhookAsync(config.Url).Wait();

            var commands = new List<Command>
            {
                new StartCommand()
            };
            Commands = commands.AsReadOnly();
        }
    }
}