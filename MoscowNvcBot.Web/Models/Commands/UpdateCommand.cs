using System;
using GoogleDocumentsUnifier.Logic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class UpdateCommand : Command
    {
        internal override string Name => "update";
        internal override string Description => "обновить раздатки на Диске";

        private readonly IEnumerable<DocumentInfo> _infos;
        private readonly string _targetId;
        private readonly string _targetUrl;
        private readonly DataManager _googleDataManager;

        public UpdateCommand(IEnumerable<string> sources, string targetId, string targetPrefix,
            DataManager googleDataManager)
        {
            _infos = sources.Select(CreateInfo);
            _targetId = targetId;
            _targetUrl = $"{targetPrefix}{targetId}";
            _googleDataManager = googleDataManager;
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            int replyToMessageId = 0;
            if (message.Chat.Type == ChatType.Group)
            {
                replyToMessageId = message.MessageId;
            }

            Task<Message> messageTask = client.SendTextMessageAsync(message.Chat, "_Обновляю..._", ParseMode.Markdown,
                replyToMessageId: replyToMessageId);

            List<Task<bool>> tasks = _infos.Select(UpdateGooglePdfAsync).ToList();
            await Task.WhenAll(tasks);
            bool updated = tasks.Select(t => t.Result).Any(r => r);

            string text = updated ? "Готово" : "Раздатки уже актуальны";
            text += $". Ссылка на папку: {_targetUrl}";

            await messageTask;
            await client.SendTextMessageAsync(message.Chat, text, replyToMessageId: replyToMessageId);
        }

        private async Task<bool> UpdateGooglePdfAsync(DocumentInfo info)
        {
            FileInfo fileInfo = await _googleDataManager.GetFileInfoAsync(info.Id);

            string pdfName = $"{fileInfo.Name}.pdf";
            FileInfo pdfInfo = await _googleDataManager.FindFileInFolderAsync(_targetId, pdfName);

            using (var temp = new TempFile())
            {
                await _googleDataManager.DownloadAsync(info, temp.File.FullName);

                return await UploadAsync(temp.File.FullName, fileInfo.ModifiedTime, pdfName, pdfInfo);
            }
        }

        private async Task<bool> UploadAsync(string sourcePath, DateTime? sourceMoifiedTime, string name,
            FileInfo target)
        {
            if (target == null)
            {
                await _googleDataManager.CreateAsync(name, _targetId, sourcePath);
                return true;
            }

            if (target.ModifiedTime < sourceMoifiedTime)
            {
                await _googleDataManager.UpdateAsync(target.Id, sourcePath);
                return true;
            }

            return false;
        }

        private static DocumentInfo CreateInfo(string source) => new DocumentInfo(source, DocumentType.Document);
    }
}
