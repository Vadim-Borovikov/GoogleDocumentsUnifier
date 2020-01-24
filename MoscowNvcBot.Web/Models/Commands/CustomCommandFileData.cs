using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class CustomCommandFileData
    {
        public readonly Task<TempFile> DownloadTask;
        public uint Amount;

        public CustomCommandFileData(Task<TempFile> downloadTask)
        {
            DownloadTask = downloadTask;
        }
    }
}
