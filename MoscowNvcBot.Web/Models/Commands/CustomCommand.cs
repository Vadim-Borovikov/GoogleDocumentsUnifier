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

        private static readonly int[] Amounts = { 0, 1, 5, 10, 20 };

        private readonly List<string> _sources;
        private readonly DataManager _googleDataManager;

        private static readonly ConcurrentDictionary<long, CustomCommandData> ChatData =
            new ConcurrentDictionary<long, CustomCommandData>();

        public CustomCommand(List<string> sources, DataManager googleDataManager)
        {
            _sources = sources;
            _googleDataManager = googleDataManager;
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            Task messageTask = SendFirstMessageAsync(message, client);

            List<string> names = await GetNamesAsync();

            await messageTask;

            var data = new CustomCommandData();
            for (int i = 0; i < _sources.Count; ++i)
            {
                var info = new DocumentInfo(_sources[i], DocumentType.GoogleDocument);
                var request = new DocumentRequest(info, 0);

                data.Documents.Add(names[i], request);

                bool isLast = i == (_sources.Count - 1);
                InlineKeyboardMarkup keyboard = GetKeyboard(request, isLast);
                await client.SendTextMessageAsync(message.Chat, names[i], disableNotification: !isLast,
                    replyMarkup: keyboard);
            }

            ChatData.AddOrUpdate(message.Chat.Id, data, (l, d) => d);
        }

        internal override async Task InvokeAsync(CallbackQuery query, ITelegramBotClient client)
        {
            long chatId = query.Message.Chat.Id;
            bool success = ChatData.TryGetValue(chatId, out CustomCommandData data);
            if (!success)
            {
                throw new Exception("Couldn't get data from ConcurrentDictionary!");
            }
            string queryData = query.Data.Replace(Name, "");
            if (queryData == "")
            {
                await GenerateAndSendAsync(client, chatId, data);
            }
            else
            {
                success = uint.TryParse(queryData, out uint amount);
                if (!success)
                {
                    throw new Exception("Couldn't get amount from query.Data!");
                }

                await UpdateAmountAsync(client, chatId, query.Message, data, amount);
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

        private async Task<List<string>> GetNamesAsync()
        {
            List<Task<FileInfo>> tasks = _sources.Select(_googleDataManager.GetFileInfoAsync).ToList();
            await Task.WhenAll(tasks);
            return tasks.Select(t => t.Result.Name).ToList();
        }

        private InlineKeyboardMarkup GetKeyboard(DocumentRequest request, bool isLast)
        {
            IEnumerable<InlineKeyboardButton> amountRow = Amounts.Select(a => GetAmountButton(a, request.Amount == a));
            if (!isLast)
            {
                return new InlineKeyboardMarkup(amountRow);
            }

            IEnumerable<InlineKeyboardButton> readyRow =
                new[] { InlineKeyboardButton.WithCallbackData("Готово!", $"{Name}") };
            var rows = new List<IEnumerable<InlineKeyboardButton>> { amountRow, readyRow };
            return new InlineKeyboardMarkup(rows);
        }

        private InlineKeyboardButton GetAmountButton(int amount, bool selected)
        {
            string text = selected ? $"• {amount} •" : $"{amount}";
            string callBackData = $"{Name}{amount}";
            return InlineKeyboardButton.WithCallbackData(text, callBackData);
        }

        private async Task GenerateAndSendAsync(ITelegramBotClient client, long chatId, CustomCommandData data)
        {
            List<DocumentRequest> requests = data.Documents.Values.ToList();
            if (requests.TrueForAll(r => r.Amount == 0))
            {
                await client.SendTextMessageAsync(chatId, "Ничего не выбрано!");
                return;
            }

            Task<Message> messageTask = client.SendTextMessageAsync(chatId, "_Готовлю..._", ParseMode.Markdown);

            string path = await UnifyRequestsAsync(requests);

            Task chatActionTask = client.SendChatActionAsync(chatId, ChatAction.UploadDocument);
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var pdf = new InputOnlineFile(fileStream, "Раздатки.pdf");
                await messageTask;
                await chatActionTask;
                await client.SendDocumentAsync(chatId, pdf);
            }

            System.IO.File.Delete(path);
        }

        private async Task UpdateAmountAsync(ITelegramBotClient client, long chatId, Message message,
            CustomCommandData data, uint amount)
        {
            string name = message.Text;
            DocumentRequest request = data.Documents[name];

            request.Amount = amount;

            bool isLast = message.ReplyMarkup.InlineKeyboard.Count() == 2;
            InlineKeyboardMarkup keyboard = GetKeyboard(request, isLast);
            await client.EditMessageTextAsync(chatId, message.MessageId, name, replyMarkup: keyboard);
        }

        private async Task<string> UnifyRequestsAsync(IEnumerable<DocumentRequest> requests)
        {
            string path = Path.GetTempFileName();

            await _googleDataManager.UnifyAsync(requests, path);

            return path;
        }
    }
}
