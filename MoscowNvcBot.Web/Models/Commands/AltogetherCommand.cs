using System;
using GoogleDocumentsUnifier.Logic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class AltogetherCommand : Command
    {
        internal override string Name => "altogether";
        internal override string Description => "все раздатки вместе";

        private const string FileName = "Все раздатки вместе.pdf";

        private static readonly TimeSpan ChatActionDuration = TimeSpan.FromSeconds(5);

        private readonly IEnumerable<DocumentRequest> _requests;
        private readonly DataManager _googleDataManager;

        public AltogetherCommand(IEnumerable<string> sources, DataManager googleDataManager)
        {
            _requests = sources.Select(CreateRequest);
            _googleDataManager = googleDataManager;
        }

        internal override async Task Execute(Message message, ITelegramBotClient client)
        {
            Task task = SendAllGooglePdfAsyncTask(client, message.Chat);
            await WrapWithChatActionAsync(task, client, message.Chat, ChatAction.UploadDocument);
        }

        private async Task SendAllGooglePdfAsyncTask(ITelegramBotClient client, Chat chat)
        {
            string path = await Task.Run(() => UnifyInfos());

            await SendFileAsync(client, chat, FileName, path);

            System.IO.File.Delete(path);
        }

        private string UnifyInfos()
        {
            string path = Path.GetTempFileName();
            _googleDataManager.Unify(_requests, path);

            return path;
        }

        private static DocumentRequest CreateRequest(string source)
        {
            var info = new DocumentInfo(source, DocumentType.GoogleDocument);
            return new DocumentRequest(info);
        }

        private static async Task WrapWithChatActionAsync(Task task, ITelegramBotClient client, Chat chat,
            ChatAction chatAction)
        {
            using (var cancellatiomTokenSource = new CancellationTokenSource())
            {
                PeriodicSendChatActionAsync(client, chat, chatAction, cancellatiomTokenSource.Token);

                await task;

                cancellatiomTokenSource.Cancel();
            }
        }

        private static async Task SendFileAsync(ITelegramBotClient client, Chat chat, string fileName, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var pdf = new InputOnlineFile(fileStream, fileName);
                await client.SendDocumentAsync(chat, pdf);
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
