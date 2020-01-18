using GoogleDocumentsUnifier.Logic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class AltogetherCommand : Command
    {
        internal override string Name => "altogether";
        internal override string Description => "все раздатки вместе";

        private const string FileName = "Все раздатки вместе.pdf";

        private readonly IEnumerable<DocumentRequest> _requests;
        private readonly DataManager _googleDataManager;

        public AltogetherCommand(IEnumerable<string> sources, DataManager googleDataManager)
        {
            _requests = sources.Select(Utils.CreateRequest);
            _googleDataManager = googleDataManager;
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            Task task = SendAllGooglePdfAsync(client, message.Chat);
            await Utils.WrapWithChatActionAsync(task, client, message.Chat, ChatAction.UploadDocument);
        }

        private async Task SendAllGooglePdfAsync(ITelegramBotClient client, Chat chat)
        {
            string path = await UnifyInfosAsync();

            await Utils.SendFileAsync(client, chat, FileName, path);

            System.IO.File.Delete(path);
        }

        private async Task<string> UnifyInfosAsync()
        {
            string path = Path.GetTempFileName();
            await _googleDataManager.UnifyAsync(_requests, path);

            return path;
        }
    }
}
