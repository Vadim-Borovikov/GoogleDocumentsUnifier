using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;

namespace MoscowNvcBot.Web.Models
{
    internal class GoogleFileData
    {
        public readonly Task<TempFile> DownloadTask;
        public uint Amount;

        public GoogleFileData(Task<TempFile> downloadTask)
        {
            DownloadTask = downloadTask;
        }
    }
}
