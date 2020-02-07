using System;
using GoogleDocumentsUnifier.Logic;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class UpdateCommand : Command
    {
        internal override string Name => "update";
        internal override string Description => "обновить раздатки на Диске";

        private readonly IEnumerable<string> _sources;
        private readonly string _targetId;
        private readonly DataManager _googleDataManager;

        public UpdateCommand(IEnumerable<string> sources, string targetId, DataManager googleDataManager)
        {
            _sources = sources;
            _targetId = targetId;
            _googleDataManager = googleDataManager;
        }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return Utils.UpdateAsync(message.Chat, client, _googleDataManager, _sources, _targetId,
                CheckGooglePdfAsync, CreateOrUpdateGoogleAsync);
        }

        private static async Task<PdfData> CheckGooglePdfAsync(string id, DataManager googleDataManager,
            string parentId)
        {
            FileInfo fileInfo = await googleDataManager.GetFileInfoAsync(id);

            string pdfName = $"{fileInfo.Name}.pdf";
            FileInfo pdfInfo = await googleDataManager.FindFileInFolderAsync(parentId, pdfName);

            if (pdfInfo == null)
            {
                return PdfData.CreateNone(id, pdfName);
            }

            if (pdfInfo.ModifiedTime < fileInfo.ModifiedTime)
            {
                return PdfData.CreateOutdated(id, pdfInfo.Id);
            }

            return PdfData.CreateOk();
        }

        private static async Task CreateOrUpdateGoogleAsync(PdfData data, DataManager googleDataManager,
            string parentId)
        {
            var info = new DocumentInfo(data.SourceId, DocumentType.Document);
            using (TempFile temp = await googleDataManager.DownloadAsync(info))
            {
                switch (data.Status)
                {
                    case PdfData.FileStatus.None:
                        await googleDataManager.CreateAsync(data.Name, parentId, temp.Path);
                        break;
                    case PdfData.FileStatus.Outdated:
                        await googleDataManager.UpdateAsync(data.IdOrPath, temp.Path);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(data.Status), data.Status,
                            "Unexpected Pdf status!");
                }
            }
        }
    }
}
