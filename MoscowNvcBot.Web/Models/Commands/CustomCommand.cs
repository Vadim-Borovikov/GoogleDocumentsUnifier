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
using File = System.IO.File;
using FileInfo = GoogleDocumentsUnifier.Logic.FileInfo;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class CustomCommand : Command
    {
        internal override string Name => "custom";
        internal override string Description => "обновить, выбрать и объединить раздатки";

        private static readonly uint[] Amounts = { 0, 1, 5, 10, 20 };

        private readonly List<string> _sourceIds;
        private readonly string _pdfFolderPath;
        private readonly DataManager _googleDataManager;

        private static readonly ConcurrentDictionary<long, CustomCommandData> ChatData =
            new ConcurrentDictionary<long, CustomCommandData>();

        public CustomCommand(List<string> sourceIds, string pdfFolderPath, DataManager googleDataManager)
        {
            _sourceIds = sourceIds;
            _pdfFolderPath = pdfFolderPath;
            _googleDataManager = googleDataManager;
            Directory.CreateDirectory(_pdfFolderPath);
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await Utils.UpdateAsync(message.Chat, client, _googleDataManager, _sourceIds, _pdfFolderPath,
                CheckLocalPdfAsync, CreateOrUpdateLocalAsync);
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

        private static async Task<PdfData> CheckLocalPdfAsync(string sourceId, DataManager googleDataManager,
            string localPath)
        {
            FileInfo fileInfo = await googleDataManager.GetFileInfoAsync(sourceId);

            string pdfName = $"{fileInfo.Name}.pdf";
            string path = Path.Combine(localPath, pdfName);
            if (!File.Exists(path))
            {
                return PdfData.CreateNone(sourceId, pdfName);
            }

            if (File.GetLastWriteTime(path) < fileInfo.ModifiedTime)
            {
                return PdfData.CreateOutdated(sourceId, path);
            }

            return PdfData.CreateOk();
        }

        private static async Task CreateOrUpdateLocalAsync(PdfData data, DataManager googleDataManager,
            string localPath)
        {
            var info = new DocumentInfo(data.SourceId, DocumentType.Document);
            string path = Path.Combine(localPath, data.Name);
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

        private async Task SelectAsync(Chat chat, ITelegramBotClient client)
        {
            Message firstMessage =
                await client.SendTextMessageAsync(chat, "Выбери раздатки:", ParseMode.Markdown,
                    disableNotification: true);

            string[] files = Directory.GetFiles(_pdfFolderPath);

            CustomCommandData data = await CreateOrClearDataAsync(client, chat.Id);
            string last = files.Last();

            data.MessageIds.Add(firstMessage.MessageId);

            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);

                var requset = new DocumentRequest(file, 0);
                data.Requests.Add(name, requset);

                bool isLast = file == last;
                InlineKeyboardMarkup keyboard = GetKeyboard(0, isLast);
                Message chatMessage =
                    await client.SendTextMessageAsync(chat, name, disableNotification: !isLast, replyMarkup: keyboard);
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

        private static async Task<bool> GenerateAndSendAsync(ITelegramBotClient client, long chatId,
            CustomCommandData data)
        {
            List<DocumentRequest> requests = data.Requests.Values.Where(f => f.Amount > 0).ToList();
            if (!requests.Any())
            {
                await client.SendTextMessageAsync(chatId, "Ничего не выбрано!");
                return false;
            }

            Message unifyingMessage = await client.SendTextMessageAsync(chatId, "_Объединяю…_", ParseMode.Markdown);

            using (TempFile temp = DataManager.Unify(requests))
            {
                await Utils.FinalizeStatusMessageAsync(unifyingMessage, client);
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

            data.Requests[name].Amount = amount;

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
    }
}
