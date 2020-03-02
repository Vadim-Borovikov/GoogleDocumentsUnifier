using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class LinksCommand : Command
    {
        internal override string Name => "links";
        internal override string Description => "полезные ссылки";

        public LinksCommand(IEnumerable<BotConfiguration.Link> links)
        {
            _links = links;
        }

        protected override async Task ExecuteAsync(Message message, ITelegramBotClient client, bool _)
        {
            foreach (BotConfiguration.Link link in _links)
            {
                if (link.MakeButton)
                {
                    InlineKeyboardMarkup keyboard = GetReplyMarkup(link);
                    await client.SendTextMessageAsync(message.Chat, link.Name, replyMarkup: keyboard);
                }
                else
                {
                    string text = $"[{link.Name}]({link.Url})";
                    await client.SendTextMessageAsync(message.Chat, text, ParseMode.Markdown);
                }
            }
        }

        private static InlineKeyboardMarkup GetReplyMarkup(BotConfiguration.Link link)
        {
            var button = new InlineKeyboardButton
            {
                Text = "Открыть",
                Url = link.Url
            };
            return new InlineKeyboardMarkup(button);
        }

        private readonly IEnumerable<BotConfiguration.Link> _links;
    }
}
