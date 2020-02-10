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

        private readonly IEnumerable<string> _sourceIds;
        private readonly string _pdfFolderId;
        private readonly DataManager _googleDataManager;

        public UpdateCommand(IEnumerable<string> sourceIds, string pdfFolderId, DataManager googleDataManager)
        {
            _sourceIds = sourceIds;
            _pdfFolderId = pdfFolderId;
            _googleDataManager = googleDataManager;
        }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return Utils.UpdateAsync(message.Chat, client, _googleDataManager, _sourceIds, _pdfFolderId,
                CheckGooglePdfAsync, CreateOrUpdateGoogleAsync);
        }

        private static async Task<PdfData> CheckGooglePdfAsync(string sourceId, DataManager googleDataManager,
            string parentId)
        {
            FileInfo fileInfo = await googleDataManager.GetFileInfoAsync(sourceId);

            string pdfName = $"{fileInfo.Name}.pdf";
            FileInfo pdfInfo = await googleDataManager.FindFileInFolderAsync(parentId, pdfName);

            if (pdfInfo == null)
            {
                return PdfData.CreateNone(sourceId, pdfName);
            }

            if (pdfInfo.ModifiedTime < fileInfo.ModifiedTime)
            {
                return PdfData.CreateOutdated(sourceId, pdfInfo.Id);
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
