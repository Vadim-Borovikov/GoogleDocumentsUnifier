using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Models
{
    internal class CustomCommandData
    {
        public readonly Dictionary<string, DocumentRequest> Requests = new Dictionary<string, DocumentRequest>();

        public Task Clear(ITelegramBotClient client, long chatId)
        {
            Requests.Clear();

            List<Task> tasks = _messageIds.Select(id => client.DeleteMessageAsync(chatId, id)).ToList();
            _messageIds.Clear();
            return Task.WhenAll(tasks);
        }

        public void AddMessage(Message message) => _messageIds.Add(message.MessageId);

        private readonly List<int> _messageIds = new List<int>();
    }
}
