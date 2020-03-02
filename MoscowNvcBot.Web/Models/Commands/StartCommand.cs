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

        private readonly IReadOnlyList<Command> _commands;

        public StartCommand(IReadOnlyList<Command> commands)
        {
            _commands = commands;
        }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Привет!");
            builder.AppendLine();
            foreach (Command command in _commands)
            {
                builder.AppendLine($"/{command.Name} – {command.Description}");
            }

            return client.SendTextMessageAsync(message.Chat, builder.ToString());
        }
    }
}
