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

        public async Task Clear(ITelegramBotClient client, long chatId)
        {
            Parallel.ForEach(Files.Values.Select(f => f.DownloadTask), t => t.Result.Dispose());
            Files.Clear();

            IEnumerable<Task> tasks = MessageIds.Select(id => client.DeleteMessageAsync(chatId, id));
            await Task.WhenAll(tasks);
            MessageIds.Clear();
        }
    }
}
