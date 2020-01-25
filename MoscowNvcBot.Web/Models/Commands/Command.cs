using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoscowNvcBot.Web.Models.Commands
{
    public abstract class Command
    {
        internal abstract string Name { get; }
        internal abstract string Description { get; }

        internal bool Contains(Message message) => (message.Type == MessageType.Text) && message.Text.Contains(Name);

        internal abstract Task ExecuteAsync(Message message, ITelegramBotClient client);
    }
}
