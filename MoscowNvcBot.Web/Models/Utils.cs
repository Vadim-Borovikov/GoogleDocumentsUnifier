using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;
using FileInfo = GoogleDocumentsUnifier.Logic.FileInfo;

namespace MoscowNvcBot.Web.Models
{
    internal static class Utils
    {
        internal static async Task<Message> FinalizeStatusMessageAsync(Message message, ITelegramBotClient client,
            string postfix = "")
        {
            Chat chat = message.Chat;
            string text = $"_{message.Text}_ Готово.{postfix}";
            await client.DeleteMessageAsync(chat, message.MessageId);
            return await client.SendTextMessageAsync(chat, text, ParseMode.Markdown);
        }

        internal static async Task<List<PdfData>> CheckAsync(IEnumerable<string> sources,
            Func<string, Task<PdfData>> check)
        {
            PdfData[] datas = await Task.WhenAll(sources.Select(check));
            return datas.Where(d => d.Status != PdfData.FileStatus.Ok).ToList();
        }

        internal static async Task CreateOrUpdateAsync(IEnumerable<PdfData> sources,
            Func<PdfData, Task> createOrUpdate)
        {
            List<Task> updateTasks = sources.Select(createOrUpdate).ToList();
            await Task.WhenAll(updateTasks);
        }

        internal static async Task<PdfData> CheckLocalPdfAsync(string sourceId, DataManager googleDataManager,
            string pdfFolderPath)
        {
            FileInfo fileInfo = await googleDataManager.GetFileInfoAsync(sourceId);

            string path = Path.Combine(pdfFolderPath, $"{fileInfo.Name}.pdf");
            if (!File.Exists(path))
            {
                return PdfData.CreateNoneLocal(sourceId, path);
            }

            if (File.GetLastWriteTime(path) < fileInfo.ModifiedTime)
            {
                return PdfData.CreateOutdatedLocal(sourceId, path);
            }

            return PdfData.CreateOk();
        }

        internal static async Task CreateOrUpdateLocalAsync(PdfData data, DataManager googleDataManager,
            string pdfFolderPath)
        {
            var info = new DocumentInfo(data.SourceId, DocumentType.Document);
            string path = Path.Combine(pdfFolderPath, data.Name);
            switch (data.Status)
            {
                case PdfData.FileStatus.None:
                case PdfData.FileStatus.Outdated:
                    await googleDataManager.DownloadAsync(info, path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(data.Status), data.Status, "Unexpected Pdf status!");
            }
        }

    }
}
