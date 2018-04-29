using GoogleDocumentsUnifier.Logic;
using System.IO;
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

        private readonly GoogleApisDriveProvider _googleProvider;

        private readonly string _checklistId;
        private readonly string _casesId;
        private readonly string _empathyId;

        public MoscowNvcBotLogc(string token, string checklistId, string casesId, string empathyId,
                                GoogleApisDriveProvider googleProvider)
        {
            _googleProvider = googleProvider;

            _checklistId = checklistId;
            _casesId = casesId;
            _empathyId = empathyId;

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

            FileToSend pdf;

            switch (e.Message.Text)
            {
                case "/start":
                    await ShowOptions(e.Message.Chat.Id);
                    break;
                case "Подготовка":
                    await SendGooglePdf(e.Message.Chat.Id, "Подготовка.pdf", _checklistId);
                    break;
                case "Разбор случаев":
                    await SendGooglePdf(e.Message.Chat.Id, "Разбор случаев.pdf", _casesId);
                    break;
                case "Эмпатия":
                    await SendGooglePdf(e.Message.Chat.Id, "Эмпатия.pdf", _empathyId);
                    break;
            }
        }

        private async Task SendGooglePdf(long chatId, string name, string pdfId)
        {
            await Bot.SendChatActionAsync(chatId, ChatAction.UploadDocument);

            string path = Path.GetTempFileName();
            using (var downloadStream = new MemoryStream())
            {
                _googleProvider.DownloadFile(pdfId, downloadStream);
                File.WriteAllBytes(path, downloadStream.ToArray());
            }

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
