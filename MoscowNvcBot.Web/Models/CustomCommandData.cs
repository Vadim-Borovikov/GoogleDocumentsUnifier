using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MoscowNvcBot.Web.Models
{
    internal class CustomCommandData
    {
        public readonly Dictionary<string, GoogleFileData> Files = new Dictionary<string, GoogleFileData>();

        public void Clear()
        {
            Parallel.ForEach(Files.Values.Select(f => f.DownloadTask), t => t.Result.Dispose());
            Files.Clear();
        }
    }
}
