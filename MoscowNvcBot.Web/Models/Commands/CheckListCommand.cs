using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class CheckListCommand : Command
    {
        internal override string Name => "checklist";
        internal override string Description => "инструкция после вступления";

        private readonly string _text;

        public CheckListCommand(string text) { _text = text; }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return client.SendTextMessageAsync(message.Chat, _text);
        }
    }
}
