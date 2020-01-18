using GoogleDocumentsUnifier.Logic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class SeparatelyCommand : Command
    {
        internal override string Name => "separately";
        internal override string Description => "все раздатки по отдельности";

        private readonly IEnumerable<DocumentRequest> _requests;
        private readonly DataManager _googleDataManager;

        public SeparatelyCommand(IEnumerable<string> sources, DataManager googleDataManager)
        {
            _requests = sources.Select(Utils.CreateRequest);
            _googleDataManager = googleDataManager;
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            IEnumerable<Task> tasks = _requests.Select(request => SendGooglePdfAsync(message.Chat, client, request));
            await Task.WhenAll(tasks);
        }

        private async Task SendGooglePdfAsync(Chat chat, ITelegramBotClient client, DocumentRequest request)
        {
            Task task = SendGooglePdfAsyncTask(client, chat, request);

            await Utils.WrapWithChatActionAsync(task, client, chat, ChatAction.UploadDocument);
        }

        private async Task SendGooglePdfAsyncTask(ITelegramBotClient client, Chat chat, DocumentRequest request)
        {
            Task<string> fileNameTask = GetNameAsync(request.Info);
            Task<string> pathTask = CopyRequest(request);

            await Task.WhenAll(fileNameTask, pathTask);

            string fileName = fileNameTask.Result;
            string path = pathTask.Result;

            await Utils.SendFileAsync(client, chat, fileName, path);

            File.Delete(path);
        }

        private async Task<string> GetNameAsync(DocumentInfo info)
        {
            string name = await _googleDataManager.GetNameAsync(info.Id);
            name = name.Replace("«", "");
            name = name.Replace("»", "");
            return $"{name}.pdf";
        }

        private async Task<string> CopyRequest(DocumentRequest request)
        {
            string path = Path.GetTempFileName();
            await _googleDataManager.CopyAsync(request, path);
            return path;
        }
    }
}
