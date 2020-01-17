using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class StartCommand : Command
    {
        internal override string Name => "start";
        internal override string Description => "список команд";

        private readonly string _startMessagePrefix;
        private readonly IReadOnlyList<Command> _commands;

        public StartCommand(string startMessagePrefix, IReadOnlyList<Command> commands)
        {
            _startMessagePrefix = startMessagePrefix;
            _commands = commands;
        }

        internal override async Task Execute(Message message, ITelegramBotClient client)
        {
            var builder = new StringBuilder(_startMessagePrefix);
            foreach (Command command in _commands)
            {
                builder.AppendLine($"/{command.Name} – {command.Description}\n");
            }

            await client.SendTextMessageAsync(message.Chat, builder.ToString());
        }
    }
}
