using System.Collections.Generic;
using GoogleDocumentsUnifier.Logic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace MoscowNvcBot.Console
{
    internal class MoscowNvcBotLogc
    {
        public readonly TelegramBotClient Bot;

        private readonly DataManager _googleDataManager;

        private readonly Dictionary<string, DocumentInfo> _sources;

        public MoscowNvcBotLogc(string token, Dictionary<string, DocumentInfo> sources,
                                DataManager googleDataManager)
        {
            _googleDataManager = googleDataManager;

            _sources = sources;

            Bot = new TelegramBotClient(token);
            Bot.OnMessage += OnMessageReceived;
        }

        private async void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message.Type != MessageType.TextMessage)
            {
                return;
            }

            System.Console.WriteLine(e.Message.Text);

            switch (e.Message.Text)
            {
                case "/start":
                    await ShowOptions(e.Message.Chat.Id);
                    break;
                case "Все раздатки вместе":
                    string[] names = new[]
                    {
                        "checklist",
                        "landing",
                        "cases_manual",
                        "cases_template",
                        "empathy_manual",
                        "feelings",
                        "needs"
                    };
                    await SendGooglePdf(e.Message.Chat.Id, "Все раздатки вместе.pdf", names);
                    break;
                case "Все раздатки по отдельности":
                    string name = "checklist";
                    await SendGooglePdf(e.Message.Chat.Id, "Подготовка к занятию.pdf", name);
                    name = "landing";
                    await SendGooglePdf(e.Message.Chat.Id, "Памятка.pdf", name);
                    name = "cases_manual";
                    await SendGooglePdf(e.Message.Chat.Id, "Как выразить своё состояние.pdf", name);
                    name = "cases_template";
                    await SendGooglePdf(e.Message.Chat.Id, "Выражаем своё состояние.pdf", name);
                    name = "empathy_manual";
                    await SendGooglePdf(e.Message.Chat.Id, "Как поняти собеседника.pdf", name);
                    name = "feelings";
                    await SendGooglePdf(e.Message.Chat.Id, "Перечень чувств.pdf", name);
                    name = "needs";
                    await SendGooglePdf(e.Message.Chat.Id, "Перечень нужд.pdf", name);
                    break;
            }
        }

        private async Task SendGooglePdf(long chatId, string fileName, IEnumerable<string> names)
        {
            await Bot.SendChatActionAsync(chatId, ChatAction.UploadDocument);

            IEnumerable<DocumentRequest> requests =
                names.Select(n => new DocumentRequest(_sources[n], 1));

            string path = Path.GetTempFileName();
            _googleDataManager.Unify(requests, path, false);

            await SendFile(chatId, fileName, path);
        }

        private async Task SendGooglePdf(long chatId, string fileName, string name)
        {
            await Bot.SendChatActionAsync(chatId, ChatAction.UploadDocument);

            DocumentRequest request = new DocumentRequest(_sources[name], 1);

            string path = Path.GetTempFileName();
            _googleDataManager.Copy(request, path, false);

            await SendFile(chatId, fileName, path);
        }

        private async Task SendFile(long chatId, string fileName, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var pdf = new FileToSend(fileName, fileStream);
                await Bot.SendDocumentAsync(chatId, pdf);
            }
            File.Delete(path);
        }

        private async Task ShowOptions(long chatId)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Все раздатки вместе"),
                new KeyboardButton("Все раздатки по отдельности")
            }, true);

            const string Message = "Выберите PDF:";

            await Bot.SendTextMessageAsync(chatId, Message, replyMarkup: replyKeyboard);
        }
    }
}
