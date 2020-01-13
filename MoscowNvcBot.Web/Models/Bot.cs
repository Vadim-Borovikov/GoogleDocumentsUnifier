using System.Collections.Generic;
using System.Threading.Tasks;
using MoscowNvcBot.Web.Models.Commands;
using Telegram.Bot;

namespace MoscowNvcBot.Web.Models
{
    internal static class Bot
    {
        private static TelegramBotClient _client;

        internal static IReadOnlyList<Command> Commands { get; private set; }

        internal static async Task<TelegramBotClient> GetBotClientAsync()
        {
            if (_client == null)
            {
                await Initialize();
            }

            return _client;
        }

        private static async Task Initialize()
        {
            var commands = new List<Command>
            {
                new StartCommand()
            };
            Commands = commands.AsReadOnly();

            _client = new TelegramBotClient(AppSettings.Key);
            await _client.SetWebhookAsync(AppSettings.Url);
        }
    }
}