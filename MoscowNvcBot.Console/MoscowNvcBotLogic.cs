using System;
using System.Collections.Generic;
using GoogleDocumentsUnifier.Logic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
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

        private static readonly TimeSpan ChatActionDuration = TimeSpan.FromSeconds(5);

        public MoscowNvcBotLogic(string token, List<DocumentInfo> infos, DataManager googleDataManager)
        {
            _googleDataManager = googleDataManager;

            _infos = infos;

            Bot = new TelegramBotClient(token);
            Bot.DeleteWebhookAsync().Wait();
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
                    await SendAllGooglePdfAsync(e.Message.Chat.Id, "Все раздатки вместе.pdf");
                    break;
                case "Все раздатки по отдельности":
                    foreach (DocumentInfo info in _infos)
                    {
                        await SendGooglePdfAsync(e.Message.Chat.Id, info);
                    }
                    break;
            }
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

        private async Task SendAllGooglePdfAsync(long chatId, string fileName)
        {
            Task task = SendAllGooglePdfAsyncTask(chatId, fileName);

            await WrapWithChatActionAsync(task, chatId, ChatAction.UploadDocument);
        }

        private async Task SendGooglePdfAsync(long chatId, DocumentInfo info)
        {
            Task task = SendGooglePdfAsyncTask(chatId, info);

            await WrapWithChatActionAsync(task, chatId, ChatAction.UploadDocument);
        }

        private async Task SendAllGooglePdfAsyncTask(long chatId, string fileName)
        {
            string path = await Task.Run(() => UnifyInfos());

            await SendFileAsync(chatId, fileName, path);

            File.Delete(path);
        }

        private async Task WrapWithChatActionAsync(Task task, ChatId chatId, ChatAction chatAction)
        {
            using (var cancellatiomTokenSource = new CancellationTokenSource())
            {
                PeriodicSendChatActionAsync(chatId, chatAction, cancellatiomTokenSource.Token);

                await task;

                cancellatiomTokenSource.Cancel();
            }
        }

        private async Task SendGooglePdfAsyncTask(long chatId, DocumentInfo info)
        {
            string fileName = await Task.Run(() => GetName(info));
            string path = await Task.Run(() => CopyInfo(info));

            await SendFileAsync(chatId, fileName, path);

            File.Delete(path);
        }

        private string UnifyInfos()
        {
            IEnumerable<DocumentRequest> requests = _infos.Select(info => new DocumentRequest(info, 1));

            string path = Path.GetTempFileName();
            _googleDataManager.Unify(requests, path, false);

            return path;
        }

        private async Task SendFileAsync(long chatId, string fileName, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var pdf = new InputOnlineFile(fileStream, fileName);
                await Bot.SendDocumentAsync(chatId, pdf);
            }
        }

        private async void PeriodicSendChatActionAsync(ChatId chatId, ChatAction chatAction,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.WhenAny(Bot.SendChatActionAsync(chatId, chatAction, cancellationToken));
                await Task.WhenAny(Task.Delay(ChatActionDuration, cancellationToken));
            }
        }

        private string CopyInfo(DocumentInfo info)
        {
            var request = new DocumentRequest(info, 1);

            string path = Path.GetTempFileName();
            _googleDataManager.Copy(request, path, false);

            return path;
        }

        private string GetName(DocumentInfo info)
        {
            string name = _googleDataManager.GetName(info.Id);
            name = name.Replace("«", "");
            name = name.Replace("»", "");
            return $"{name}.pdf";
        }
    }
}
