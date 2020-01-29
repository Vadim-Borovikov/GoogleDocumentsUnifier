﻿using System;
using System.Collections.Concurrent;
using GoogleDocumentsUnifier.Logic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using FileInfo = GoogleDocumentsUnifier.Logic.FileInfo;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class CustomCommand : Command
    {
        internal override string Name => "custom";
        internal override string Description => "обновить, выбрать и объединить раздатки";

        private static readonly uint[] Amounts = { 0, 1, 5, 10, 20 };

        private readonly List<string> _documentIds;
        private readonly string _folderId;
        private readonly DataManager _googleDataManager;

        private static readonly ConcurrentDictionary<long, CustomCommandData> ChatData =
            new ConcurrentDictionary<long, CustomCommandData>();

        public CustomCommand(List<string> documentIds, string folderId, DataManager googleDataManager)
        {
            _documentIds = documentIds;
            _folderId = folderId;
            _googleDataManager = googleDataManager;
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await Utils.UpdateAsync(message.Chat, client, _googleDataManager, _documentIds, _folderId);
            await SelectAsync(message.Chat, client);
        }

        internal override async Task InvokeAsync(Message message, ITelegramBotClient client, string data)
        {
            long chatId = message.Chat.Id;
            bool success = ChatData.TryGetValue(chatId, out CustomCommandData commandData);
            if (!success)
            {
                throw new Exception("Couldn't get data from ConcurrentDictionary!");
            }

            if (data == "")
            {
                bool shouldCleanup = await GenerateAndSendAsync(client, chatId, commandData);
                if (shouldCleanup)
                {
                    await commandData.Clear(client, message.Chat.Id);
                }
            }
            else
            {
                success = uint.TryParse(data, out uint amount);
                if (!success)
                {
                    throw new Exception("Couldn't get amount from query.Data!");
                }

                await UpdateAmountAsync(client, chatId, message, commandData, amount);
            }
        }

        internal override async Task HandleExceptionAsync(Exception exception, long chatId, ITelegramBotClient client)
        {
            bool success = ChatData.TryGetValue(chatId, out CustomCommandData data);
            if (!success)
            {
                throw new Exception("Couldn't get data from ConcurrentDictionary!");
            }
            await data.Clear(client, chatId);

            await base.HandleExceptionAsync(exception, chatId, client);
        }

        private async Task SelectAsync(Chat chat, ITelegramBotClient client)
        {
            Message firstMessage =
                await client.SendTextMessageAsync(chat, "Выбери раздатки:", ParseMode.Markdown,
                    disableNotification: true);

            IEnumerable<FileInfo> infos = await _googleDataManager.GetFilesInFolderAsync(_folderId);
            List<FileInfo> infosList = infos.ToList();

            CustomCommandData data = await CreateOrClearDataAsync(client, chat.Id);
            FileInfo last = infosList.Last();

            data.MessageIds.Add(firstMessage.MessageId);

            foreach (FileInfo info in infosList)
            {
                string name = Path.GetFileNameWithoutExtension(info.Name);
                var docInfo = new DocumentInfo(info.Id, DocumentType.Pdf);

                Task<TempFile> downloadTask = _googleDataManager.DownloadAsync(docInfo);

                var fileData = new GoogleFileData(downloadTask);
                data.Files.Add(name, fileData);

                bool isLast = info == last;
                InlineKeyboardMarkup keyboard = GetKeyboard(0, isLast);
                Message chatMessage = await client.SendTextMessageAsync(chat, name,
                    disableNotification: !isLast, replyMarkup: keyboard);
                data.MessageIds.Add(chatMessage.MessageId);
            }
        }

        private static async Task<CustomCommandData> CreateOrClearDataAsync(ITelegramBotClient client, long chatId)
        {
            bool found = ChatData.TryGetValue(chatId, out CustomCommandData data);
            if (found)
            {
                await data.Clear(client, chatId);
            }
            else
            {
                data = new CustomCommandData();
            }

            ChatData.AddOrUpdate(chatId, data, (l, d) => d);
            return data;
        }

        private InlineKeyboardMarkup GetKeyboard(uint amount, bool isLast)
        {
            IEnumerable<InlineKeyboardButton> amountRow = Amounts.Select(a => GetAmountButton(a, amount == a));
            if (!isLast)
            {
                return new InlineKeyboardMarkup(amountRow);
            }

            IEnumerable<InlineKeyboardButton> readyRow =
                new[] { InlineKeyboardButton.WithCallbackData("Готово!", $"{Name}") };
            var rows = new List<IEnumerable<InlineKeyboardButton>> { amountRow, readyRow };
            return new InlineKeyboardMarkup(rows);
        }

        private async Task<bool> GenerateAndSendAsync(ITelegramBotClient client, long chatId, CustomCommandData data)
        {
            List<GoogleFileData> files = data.Files.Values.Where(f => f.Amount > 0).ToList();
            if (!files.Any())
            {
                await client.SendTextMessageAsync(chatId, "Ничего не выбрано!");
                return false;
            }

            List<Task<TempFile>> tasks = files.Select(f => f.DownloadTask).ToList();

            List<Task<TempFile>> runningTasks = tasks.Where(t => t.Status == TaskStatus.Running).ToList();
            if (runningTasks.Any())
            {
                Message downloadingMessage =
                    await client.SendTextMessageAsync(chatId, "_Докачиваю…_", ParseMode.Markdown);
                await Task.WhenAll(runningTasks);
                await client.EditMessageTextAsync(chatId, downloadingMessage.MessageId, "_Докачиваю…_ Готово.",
                    ParseMode.Markdown);
            }

            if (tasks.Any(t => t.Status != TaskStatus.RanToCompletion))
            {
                await client.SendTextMessageAsync(chatId,
                    $"Ой, что-то не вышло! Попробуй ещё раз, пожалуйста: /{Name}");
                return true;
            }

            Message unifyingMessage = await client.SendTextMessageAsync(chatId, "_Объединяю…_", ParseMode.Markdown);

            using (TempFile temp = DataManager.Unify(files.Select(CreateRequest)))
            {
                await client.EditMessageTextAsync(chatId, unifyingMessage.MessageId, "_Объединяю…_ Готово.",
                    ParseMode.Markdown);
                await client.SendChatActionAsync(chatId, ChatAction.UploadDocument);
                using (var fileStream = new FileStream(temp.Path, FileMode.Open))
                {
                    var pdf = new InputOnlineFile(fileStream, "Раздатки.pdf");
                    await client.SendDocumentAsync(chatId, pdf);
                }
            }

            return true;
        }

        private Task<Message> UpdateAmountAsync(ITelegramBotClient client, long chatId, Message message,
            CustomCommandData data, uint amount)
        {
            string name = message.Text;

            data.Files[name].Amount = amount;

            bool isLast = message.ReplyMarkup.InlineKeyboard.Count() == 2;
            InlineKeyboardMarkup keyboard = GetKeyboard(amount, isLast);
            return client.EditMessageReplyMarkupAsync(chatId, message.MessageId, keyboard);
        }

        private InlineKeyboardButton GetAmountButton(uint amount, bool selected)
        {
            string text = selected ? $"• {amount} •" : $"{amount}";
            string callBackData = $"{Name}{amount}";
            return InlineKeyboardButton.WithCallbackData(text, callBackData);
        }

        private static DocumentRequest CreateRequest(GoogleFileData data)
        {
            if (!data.DownloadTask.IsCompletedSuccessfully)
            {
                throw new Exception("File should be downloaded already!");
            }
            return new DocumentRequest(data.DownloadTask.Result.Path, data.Amount);
        }
    }
}
