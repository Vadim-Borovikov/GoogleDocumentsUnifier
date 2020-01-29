using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoscowNvcBot.Web.Models
{
    internal static class Utils
    {
        internal static async Task UpdateAsync(Chat chat, ITelegramBotClient client, DataManager googleDataManager,
            IEnumerable<string> documentIds, string parentId)
        {
            Message checkingMessage =
                await client.SendTextMessageAsync(chat, "_Проверяю…_", ParseMode.Markdown);

            GooglePdfData[] datas =
                await Task.WhenAll(documentIds.Select(id => CheckGooglePdfAsync(id, googleDataManager, parentId)));

            List<GooglePdfData> filesToUpdate = datas.Where(d => d.Status != GooglePdfData.FileStatus.Ok).ToList();

            if (filesToUpdate.Any())
            {
                await client.EditMessageTextAsync(chat, checkingMessage.MessageId, "_Проверяю…_ Готово.",
                    ParseMode.Markdown);

                Message updatingMessage = await client.SendTextMessageAsync(chat, "_Обновляю…_", ParseMode.Markdown);

                IEnumerable<Task> updateTasks =
                    filesToUpdate.Select(f => CreateOrUpdateAsync(f, googleDataManager, parentId));
                await Task.WhenAll(updateTasks);

                await client.EditMessageTextAsync(chat, updatingMessage.MessageId, "_Обновляю…_ Готово.",
                    ParseMode.Markdown);
            }
            else
            {
                await client.EditMessageTextAsync(chat, checkingMessage.MessageId,
                    "_Проверяю…_ Готово. Раздатки уже актуальны.", ParseMode.Markdown);
            }
        }

        private static async Task<GooglePdfData> CheckGooglePdfAsync(string id, DataManager googleDataManager,
            string parentId)
        {
            FileInfo fileInfo = await googleDataManager.GetFileInfoAsync(id);

            string pdfName = $"{fileInfo.Name}.pdf";
            FileInfo pdfInfo = await googleDataManager.FindFileInFolderAsync(parentId, pdfName);

            if (pdfInfo == null)
            {
                return GooglePdfData.CreateNone(id, pdfName);
            }

            if (pdfInfo.ModifiedTime < fileInfo.ModifiedTime)
            {
                return GooglePdfData.CreateOutdated(id, pdfInfo.Id);
            }

            return GooglePdfData.CreateOk();
        }

        private static async Task CreateOrUpdateAsync(GooglePdfData data, DataManager googleDataManager,
            string parentId)
        {
            var info = new DocumentInfo(data.SourceId, DocumentType.Document);
            using (TempFile temp = await googleDataManager.DownloadAsync(info))
            {
                switch (data.Status)
                {
                    case GooglePdfData.FileStatus.None:
                        await googleDataManager.CreateAsync(data.Name, parentId, temp.Path);
                        break;
                    case GooglePdfData.FileStatus.Outdated:
                        await googleDataManager.UpdateAsync(data.Id, temp.Path);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(data.Status), data.Status,
                            "Unexpected Pdf status!");
                }
            }
        }
    }
}
