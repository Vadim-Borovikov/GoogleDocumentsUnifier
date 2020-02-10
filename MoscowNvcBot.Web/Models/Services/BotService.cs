using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;
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

        private readonly DataManager _googleDataManager;

        public BotService(IOptions<BotConfiguration> options)
        {
            _config = options.Value;

            Client = new TelegramBotClient(_config.Token);

            _googleDataManager = new DataManager(_config.GoogleProjectJson);

            var commands = new List<Command>
            {
                new CustomCommand(_config.DocumentIds, _config.PdfFolderPath, _googleDataManager),
                new UpdateCommand(_config.DocumentIds, _config.PdfFolderId, _googleDataManager),
                new CheckListCommand(_config.CheckList)
            };

            Commands = commands.AsReadOnly();
            var startCommand = new StartCommand(Commands, _config.Host);

            commands.Insert(0, startCommand);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Client.SetWebhookAsync(_config.Url, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _googleDataManager.Dispose();
            return Client.DeleteWebhookAsync(cancellationToken);
        }
    }
}