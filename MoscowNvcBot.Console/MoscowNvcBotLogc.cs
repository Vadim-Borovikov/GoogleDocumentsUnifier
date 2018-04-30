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

            string[] names;

            switch (e.Message.Text)
            {
                case "/start":
                    await ShowOptions(e.Message.Chat.Id);
                    break;
                case "Подготовка":
                    names = new[] { "checklist" };
                    await SendGooglePdf(e.Message.Chat.Id, "Подготовка.pdf", names);
                    break;
                case "Разбор случаев":
                    names = new[]
                    {
                        "landing",
                        "cases_manual",
                        "cases_template",
                        "feelings",
                        "needs"
                    };
                    await SendGooglePdf(e.Message.Chat.Id, "Разбор случаев.pdf", names);
                    break;
                case "Эмпатия":
                    names = new[]
                    {
                        "landing",
                        "empathy_manual",
                        "feelings",
                        "needs"
                    };
                    await SendGooglePdf(e.Message.Chat.Id, "Эмпатия.pdf", names);
                    break;
            }
        }

        private async Task SendGooglePdf(long chatId, string name, IEnumerable<string> names)
        {
            await Bot.SendChatActionAsync(chatId, ChatAction.UploadDocument);

            IEnumerable<DocumentRequest> requests =
                names.Select(n => new DocumentRequest(_sources[n], 1));

            string path = Path.GetTempFileName();
            _googleDataManager.Unify(requests, path, false);

            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var pdf = new FileToSend(name, fileStream);
                await Bot.SendDocumentAsync(chatId, pdf);
            }
            File.Delete(path);
        }

        private async Task ShowOptions(long chatId)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Подготовка"),
                new KeyboardButton("Разбор случаев"),
                new KeyboardButton("Эмпатия")
            }, true);

            const string Message = "Выберите PDF:";

            await Bot.SendTextMessageAsync(chatId, Message, replyMarkup: replyKeyboard);
        }
    }
}
