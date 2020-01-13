using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class StartCommand : Command
    {
        protected override string Name => "start";

        internal override async Task Execute(Message message, TelegramBotClient botClient)
        {
            long chatId = message.Chat.Id;
            int messageId = message.MessageId;

            await botClient.SendTextMessageAsync(chatId, "Hello!", replyToMessageId: messageId);
        }
    }
}
