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
            foreach (DocumentRequest request in _requests)
            {
                Task task = SendGooglePdfAsyncTask(client, message.Chat, request);

                await Utils.WrapWithChatActionAsync(task, client, message.Chat, ChatAction.UploadDocument);
            }
        }

        private async Task SendGooglePdfAsyncTask(ITelegramBotClient client, Chat chat, DocumentRequest request)
        {
            string fileName = await GetNameAsync(request.Info);
            string path = await CopyRequest(request);

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
