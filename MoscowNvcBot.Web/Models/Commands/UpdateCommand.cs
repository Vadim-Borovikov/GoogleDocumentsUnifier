﻿using System;
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

        private readonly IEnumerable<string> _sources;
        private readonly string _targetId;
        private readonly string _targetUrl;
        private readonly DataManager _googleDataManager;

        public UpdateCommand(IEnumerable<string> sources, string targetId, string targetPrefix,
            DataManager googleDataManager)
        {
            _sources = sources;
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

            Task<Message> messageTask = client.SendTextMessageAsync(message.Chat, "_Проверяю..._", ParseMode.Markdown,
                replyToMessageId: replyToMessageId);

            List<Task<GooglePdfData>> checkTasks = _sources.Select(CheckGooglePdfAsync).ToList();
            await Task.WhenAll(checkTasks);
            List<GooglePdfData> toUpdate =
                checkTasks.Select(t => t.Result).Where(d => d.Status != GooglePdfData.FileStatus.Ok).ToList();

            string text;
            if (toUpdate.Any())
            {
                await messageTask;
                messageTask = client.SendTextMessageAsync(message.Chat, "_Обновляю..._", ParseMode.Markdown,
                    replyToMessageId: replyToMessageId);

                IEnumerable<Task> updateTasks = toUpdate.Select(UpdateAsync);
                await Task.WhenAll(updateTasks);

                text = "Готово";
            }
            else
            {
                text = "Раздатки уже актуальны";
            }

            await messageTask;
            await client.SendTextMessageAsync(message.Chat, $"{text}. Ссылка на папку: {_targetUrl}",
                replyToMessageId: replyToMessageId);
        }

        private async Task<GooglePdfData> CheckGooglePdfAsync(string id)
        {
            FileInfo fileInfo = await _googleDataManager.GetFileInfoAsync(id);

            string pdfName = $"{fileInfo.Name}.pdf";
            FileInfo pdfInfo = await _googleDataManager.FindFileInFolderAsync(_targetId, pdfName);

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

        private async Task UpdateAsync(GooglePdfData data)
        {
            using (var temp = new TempFile())
            {
                var info = new DocumentInfo(data.SourceId, DocumentType.Document);
                await _googleDataManager.DownloadAsync(info, temp.File.FullName);

                switch (data.Status)
                {
                    case GooglePdfData.FileStatus.None:
                        await _googleDataManager.CreateAsync(data.Name, _targetId, temp.File.FullName);
                        break;
                    case GooglePdfData.FileStatus.Outdated:
                        await _googleDataManager.UpdateAsync(data.Id, temp.File.FullName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(GooglePdfData.FileStatus), data.Status,
                            "Unexpected Pdf status!");
                }
            }
        }
    }
}
