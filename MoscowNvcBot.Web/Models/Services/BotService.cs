using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MoscowNvcBot.Web.Models.Commands;
using Telegram.Bot;

namespace MoscowNvcBot.Web.Models.Services
{
    internal class BotService : IBotService, IHostedService
    {
        public TelegramBotClient Client { get; }
        public IReadOnlyList<Command> Commands { get; }

        private readonly BotConfiguration _config;

        public BotService(IOptions<BotConfiguration> options)
        {
            _config = options.Value;

            Client = new TelegramBotClient(_config.Token);

            var commands = new List<Command>();

            Commands = commands.AsReadOnly();
            var startCommand = new StartCommand(_config.StartMessagePrefix, Commands);

            commands.Add(startCommand);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Client.SetWebhookAsync(_config.Url, cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.DeleteWebhookAsync(cancellationToken);
        }
    }
}