using System.Collections.Generic;
using System.Linq;
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

        public StartCommand(IReadOnlyCollection<Command> commands)
        {
            _commands = commands;
        }

        protected override Task ExecuteAsync(Message message, ITelegramBotClient client, bool fromAdmin)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Привет!");
            builder.AppendLine();
            foreach (Command command in _commands.Where(c => !c.AdminsOnly || fromAdmin))
            {
                builder.AppendLine($"/{command.Name} – {command.Description}");
            }

            return client.SendTextMessageAsync(message.Chat, builder.ToString());
        }

        private readonly IReadOnlyCollection<Command> _commands;
    }
}
