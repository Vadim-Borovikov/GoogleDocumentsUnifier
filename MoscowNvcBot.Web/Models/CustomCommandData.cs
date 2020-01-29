using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MoscowNvcBot.Web.Models
{
    internal class CustomCommandData
    {
        public readonly Dictionary<string, GoogleFileData> Files = new Dictionary<string, GoogleFileData>();
        public readonly List<int> MessageIds = new List<int>();

        public Task Clear(ITelegramBotClient client, long chatId)
        {
            foreach (GoogleFileData data in Files.Values)
            {
                data.DownloadTask.ContinueWith(t => t.Result.Dispose());
            }
            Files.Clear();

            List<Task> tasks = MessageIds.Select(id => client.DeleteMessageAsync(chatId, id)).ToList();
            MessageIds.Clear();
            return Task.WhenAll(tasks);
        }
    }
}
