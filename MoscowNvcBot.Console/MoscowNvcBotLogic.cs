using System.Collections.Generic;
using GoogleDocumentsUnifier.Logic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace MoscowNvcBot.Console
{
    internal class MoscowNvcBotLogic
    {
        public readonly TelegramBotClient Bot;

        private readonly DataManager _googleDataManager;

        private readonly List<DocumentInfo> _infos;

        public MoscowNvcBotLogic(string token, List<DocumentInfo> infos, DataManager googleDataManager)
        {
            _googleDataManager = googleDataManager;

            _infos = infos;

            Bot = new TelegramBotClient(token);
            Bot.OnMessage += OnMessageReceivedAsync;
        }

        private async void OnMessageReceivedAsync(object sender, MessageEventArgs e)
        {
            if (e.Message.Type != MessageType.Text)
            {
                return;
            }

            System.Console.WriteLine(e.Message.Text);

            switch (e.Message.Text)
            {
                case "/start":
                    await ShowOptionsAsync(e.Message.Chat.Id);
                    break;
                case "Все раздатки вместе":
                    await SendGooglePdfAsync(e.Message.Chat.Id, "Все раздатки вместе.pdf", _infos);
                    break;
                case "Все раздатки по отдельности":
                    foreach (DocumentInfo info in _infos)
                    {
                        await SendGooglePdfAsync(e.Message.Chat.Id, info);
                    }
                    break;
            }
        }

        private async Task SendGooglePdfAsync(long chatId, string fileName, IEnumerable<DocumentInfo> infos)
        {
            await Bot.SendChatActionAsync(chatId, ChatAction.UploadDocument);

            IEnumerable<DocumentRequest> requests = infos.Select(info => new DocumentRequest(info, 1));

            string path = Path.GetTempFileName();
            _googleDataManager.Unify(requests, path, false);

            await SendFileAsync(chatId, fileName, path);
        }

        private async Task SendGooglePdfAsync(long chatId, DocumentInfo info)
        {
            await Bot.SendChatActionAsync(chatId, ChatAction.UploadDocument);

            var request = new DocumentRequest(info, 1);

            string path = Path.GetTempFileName();
            _googleDataManager.Copy(request, path, false);

            string fileName = GetName(info);

            await SendFileAsync(chatId, fileName, path);
        }

        private string GetName(DocumentInfo info)
        {
            string name = _googleDataManager.GetName(info.Id);
            name = name.Replace("«", "");
            name = name.Replace("»", "");
            return $"{name}.pdf";
        }

        private async Task SendFileAsync(long chatId, string fileName, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var pdf = new InputOnlineFile(fileStream, fileName);
                await Bot.SendDocumentAsync(chatId, pdf);
            }
            File.Delete(path);
        }

        private async Task ShowOptionsAsync(long chatId)
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
