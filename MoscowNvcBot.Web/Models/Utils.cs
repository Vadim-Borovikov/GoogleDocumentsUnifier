using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace MoscowNvcBot.Web.Models
{
    internal static class Utils
    {
        private static readonly TimeSpan ChatActionDuration = TimeSpan.FromSeconds(5);

        public static DocumentRequest CreateRequest(string source)
        {
            var info = new DocumentInfo(source, DocumentType.GoogleDocument);
            return new DocumentRequest(info);
        }

        public static async Task SendFileAsync(ITelegramBotClient client, Chat chat, string fileName, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var pdf = new InputOnlineFile(fileStream, fileName);
                await client.SendDocumentAsync(chat, pdf);
            }
        }

        public static async Task WrapWithChatActionAsync(Task task, ITelegramBotClient client, Chat chat,
            ChatAction chatAction)
        {
            using (var cancellatiomTokenSource = new CancellationTokenSource())
            {
                PeriodicSendChatActionAsync(client, chat, chatAction, cancellatiomTokenSource.Token);

                await task;

                cancellatiomTokenSource.Cancel();
            }
        }

        private static async void PeriodicSendChatActionAsync(ITelegramBotClient client, ChatId chatId,
            ChatAction chatAction, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.WhenAny(client.SendChatActionAsync(chatId, chatAction, cancellationToken));
                await Task.WhenAny(Task.Delay(ChatActionDuration, cancellationToken));
            }
        }
    }
}
