using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;
using Telegram.Bot;

namespace MoscowNvcBot.Web.Models
{
    internal class CustomCommandData
    {
        public readonly Dictionary<string, DocumentRequest> Requests = new Dictionary<string, DocumentRequest>();
        public readonly List<int> MessageIds = new List<int>();

        public Task Clear(ITelegramBotClient client, long chatId)
        {
            Requests.Clear();

            List<Task> tasks = MessageIds.Select(id => client.DeleteMessageAsync(chatId, id)).ToList();
            MessageIds.Clear();
            return Task.WhenAll(tasks);
        }
    }
}
