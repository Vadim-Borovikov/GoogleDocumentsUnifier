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
            return Utils.UpdateAsync(message.Chat, client, _sourceIds, CheckGooglePdfAsync, CreateOrUpdateGoogleAsync);
        }

        private async Task<PdfData> CheckGooglePdfAsync(string sourceId)
        {
            FileInfo fileInfo = await _googleDataManager.GetFileInfoAsync(sourceId);

            string pdfName = $"{fileInfo.Name}.pdf";
            FileInfo pdfInfo = await _googleDataManager.FindFileInFolderAsync(_pdfFolderId, pdfName);

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

        private async Task CreateOrUpdateGoogleAsync(PdfData data)
        {
            var info = new DocumentInfo(data.SourceId, DocumentType.Document);
            using (TempFile temp = await _googleDataManager.DownloadAsync(info))
            {
                switch (data.Status)
                {
                    case PdfData.FileStatus.None:
                        await _googleDataManager.CreateAsync(data.Name, _pdfFolderId, temp.Path);
                        break;
                    case PdfData.FileStatus.Outdated:
                        await _googleDataManager.UpdateAsync(data.IdOrPath, temp.Path);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(data.Status), data.Status,
                            "Unexpected Pdf status!");
                }
            }
        }
    }
}
