using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
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
        public IReadOnlyCollection<Command> Commands { get; }
        public IEnumerable<int> AdminIds => _config.AdminIds;

        public BotService(IOptions<BotConfiguration> options)
        {
            _config = options.Value;

            Client = new TelegramBotClient(_config.Token);

            var commands = new List<Command>
            {
                new CustomCommand(_config.DocumentIds, _config.PdfFolderPath, _googleDataManager),
                new UpdateCommand(_config.DocumentIds, _config.PdfFolderId, _config.PdfFolderPath, _googleDataManager),
                new CheckListCommand(_config.CheckList)
            };

            Commands = commands.AsReadOnly();
            var startCommand = new StartCommand(Commands);

            commands.Insert(0, startCommand);

            var uri = new Uri(_config.Url);
            _pingUrl = uri.Host;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _googleDataManager = new DataManager(_config.GoogleProjectJson);
            _periodicCancellationSource = new CancellationTokenSource();
            _ping = new Ping();
            StartPeriodicPing(_periodicCancellationSource.Token);

            return Client.SetWebhookAsync(_config.Url, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _googleDataManager.Dispose();
            _periodicCancellationSource.Cancel();
            _ping.Dispose();
            _periodicCancellationSource.Dispose();
            return Client.DeleteWebhookAsync(cancellationToken);
        }

        private void StartPeriodicPing(CancellationToken cancellationToken)
        {
            IObservable<long> observable = Observable.Interval(_config.PingPeriod);
            observable.Subscribe(PingSite, cancellationToken);
        }

        private void PingSite(long _) => _ping.Send(_pingUrl);

        private readonly BotConfiguration _config;

        private DataManager _googleDataManager;

        private CancellationTokenSource _periodicCancellationSource;
        private Ping _ping;
        private readonly string _pingUrl;
    }
}