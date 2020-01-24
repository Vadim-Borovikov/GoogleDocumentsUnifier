using System;
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
        internal override string Description => "выбрать и объединить раздатки";

        private static readonly uint[] Amounts = { 0, 1, 5, 10, 20 };

        private readonly string _sourcesUrl;
        private readonly DataManager _googleDataManager;

        private static readonly ConcurrentDictionary<long, CustomCommandData> ChatData =
            new ConcurrentDictionary<long, CustomCommandData>();

        public CustomCommand(string sourcesUrl, DataManager googleDataManager)
        {
            _sourcesUrl = sourcesUrl;
            _googleDataManager = googleDataManager;
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            Task messageTask = SendFirstMessageAsync(message, client);

            IEnumerable<FileInfo> infos = await _googleDataManager.GetFilesInFolderAsync(_sourcesUrl);
            List<FileInfo> infosList = infos.ToList();

            CustomCommandData data = CreateOrClearData(message.Chat.Id);
            FileInfo last = infosList.Last();

            await messageTask;

            foreach (FileInfo info in infosList)
            {
                string name = Path.GetFileNameWithoutExtension(info.Name);
                var docInfo = new DocumentInfo(info.Id, DocumentType.Pdf);

                Task<TempFile> downloadTask = DownloadToTempAsync(docInfo);

                var fileData = new CustomCommandFileData(downloadTask);
                data.Files.Add(name, fileData);

                bool isLast = info == last;
                InlineKeyboardMarkup keyboard = GetKeyboard(0, isLast);
                await client.SendTextMessageAsync(message.Chat, name, disableNotification: !isLast,
                    replyMarkup: keyboard);
            }
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
                await GenerateAndSendAsync(client, chatId, commandData);
                ClearData(commandData);
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

        private static async Task SendFirstMessageAsync(Message message, ITelegramBotClient client)
        {
            int replyToMessageId = 0;
            if (message.Chat.Type == ChatType.Group)
            {
                replyToMessageId = message.MessageId;
            }
            await client.SendTextMessageAsync(message.Chat, "Выбери раздатки:", ParseMode.Markdown,
                disableNotification: true, replyToMessageId: replyToMessageId);
        }

        private static CustomCommandData CreateOrClearData(long chatId)
        {
            bool found = ChatData.TryGetValue(chatId, out CustomCommandData data);
            if (found)
            {
                ClearData(data);
            }
            else
            {
                data = new CustomCommandData();
            }

            ChatData.AddOrUpdate(chatId, data, (l, d) => d);
            return data;
        }

        private async Task<TempFile> DownloadToTempAsync(DocumentInfo info)
        {
            var temp = new TempFile();
            await _googleDataManager.DownloadAsync(info, temp.File.FullName);
            return temp;
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

        private async Task GenerateAndSendAsync(ITelegramBotClient client, long chatId, CustomCommandData data)
        {
            List<CustomCommandFileData> files = data.Files.Values.Where(f => f.Amount > 0).ToList();
            if (!files.Any())
            {
                await client.SendTextMessageAsync(chatId, "Ничего не выбрано!");
                return;
            }

            List<Task<TempFile>> tasks = files.Select(f => f.DownloadTask).ToList();

            List<Task<TempFile>> runningTasks = tasks.Where(t => t.Status == TaskStatus.Running).ToList();
            Task<Message> messageTask;
            if (runningTasks.Any())
            {
                messageTask = client.SendTextMessageAsync(chatId, "_Докачиваю..._", ParseMode.Markdown);
                await Task.WhenAll(runningTasks);
                await messageTask;
            }

            if (tasks.Any(t => t.Status != TaskStatus.RanToCompletion))
            {
                await client.SendTextMessageAsync(chatId,
                    $"Ой, что-то не вышло! Попробуй ещё раз, пожалуйста: /{Name}");
                return;
            }

            messageTask = client.SendTextMessageAsync(chatId, "_Объединяю..._", ParseMode.Markdown);

            using (var temp = new TempFile())
            {
                DataManager.Unify(files.Select(CreateRequest), temp.File.FullName);

                Task chatActionTask = client.SendChatActionAsync(chatId, ChatAction.UploadDocument);
                using (var fileStream = new FileStream(temp.File.FullName, FileMode.Open))
                {
                    var pdf = new InputOnlineFile(fileStream, "Раздатки.pdf");
                    await messageTask;
                    await chatActionTask;
                    await client.SendDocumentAsync(chatId, pdf);
                }
            }
        }

        private async Task UpdateAmountAsync(ITelegramBotClient client, long chatId, Message message,
            CustomCommandData data, uint amount)
        {
            string name = message.Text;

            data.Files[name].Amount = amount;

            bool isLast = message.ReplyMarkup.InlineKeyboard.Count() == 2;
            InlineKeyboardMarkup keyboard = GetKeyboard(amount, isLast);
            await client.EditMessageTextAsync(chatId, message.MessageId, name, replyMarkup: keyboard);
        }

        private static void ClearData(CustomCommandData data)
        {
            Parallel.ForEach(data.Files.Values.Select(f => f.DownloadTask), t => t.Result.Dispose());
            data.Files.Clear();
        }

        private InlineKeyboardButton GetAmountButton(uint amount, bool selected)
        {
            string text = selected ? $"• {amount} •" : $"{amount}";
            string callBackData = $"{Name}{amount}";
            return InlineKeyboardButton.WithCallbackData(text, callBackData);
        }

        private static DocumentRequest CreateRequest(CustomCommandFileData data)
        {
            string path = data.DownloadTask.Result.File.FullName;
            return new DocumentRequest(path, data.Amount);
        }
    }
}
