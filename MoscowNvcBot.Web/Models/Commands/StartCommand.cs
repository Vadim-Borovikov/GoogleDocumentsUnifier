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

        internal override AccessType Type => AccessType.All;

        public StartCommand(IReadOnlyCollection<Command> commands)
        {
            _commands = commands;
        }

        protected override Task ExecuteAsync(Message message, ITelegramBotClient client, bool fromAdmin)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Привет!");
            builder.AppendLine();
            if (fromAdmin)
            {
                AppendCommands(builder, _commands.Where(c => c.Type != AccessType.Users));

                builder.AppendLine();
                builder.AppendLine("Команды для участников:");
            }

            AppendCommands(builder, _commands.Where(c => c.Type == AccessType.Users));

            return client.SendTextMessageAsync(message.Chat, builder.ToString());
        }

        private static void AppendCommands(StringBuilder builder, IEnumerable<Command> commands)
        {
            foreach (Command command in commands)
            {
                builder.AppendLine(GetCommandLine(command));
            }
        }
        private static string GetCommandLine(Command command) => $"/{command.Name} – {command.Description}";

        private readonly IReadOnlyCollection<Command> _commands;
    }
}
