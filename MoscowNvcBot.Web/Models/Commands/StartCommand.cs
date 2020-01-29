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
        private readonly string _url;

        public StartCommand(IReadOnlyList<Command> commands, string url)
        {
            _commands = commands;
            _url = url;
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
            builder.AppendLine();
            builder.AppendLine($"Иногда я засыпаю, но ты можешь меня разбудить, если зайдёшь на {_url}.");

            return client.SendTextMessageAsync(message.Chat, builder.ToString());
        }
    }
}
