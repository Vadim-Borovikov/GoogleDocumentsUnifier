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

        internal override async Task Execute(Message message, ITelegramBotClient client)
        {
            foreach (DocumentRequest request in _requests)
            {
                Task task = SendGooglePdfAsyncTask(client, message.Chat, request);

                await Utils.WrapWithChatActionAsync(task, client, message.Chat, ChatAction.UploadDocument);
            }
        }

        private async Task SendGooglePdfAsyncTask(ITelegramBotClient client, Chat chat, DocumentRequest request)
        {
            string fileName = await Task.Run(() => GetName(request.Info));
            string path = await Task.Run(() => CopyRequest(request));

            await Utils.SendFileAsync(client, chat, fileName, path);

            File.Delete(path);
        }

        private string GetName(DocumentInfo info)
        {
            string name = _googleDataManager.GetName(info.Id);
            name = name.Replace("«", "");
            name = name.Replace("»", "");
            return $"{name}.pdf";
        }

        private string CopyRequest(DocumentRequest request)
        {
            string path = Path.GetTempFileName();
            _googleDataManager.Copy(request, path);
            return path;
        }
    }
}
