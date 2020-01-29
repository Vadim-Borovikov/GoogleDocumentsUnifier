using GoogleDocumentsUnifier.Logic;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class UpdateCommand : Command
    {
        internal override string Name => "update";
        internal override string Description => "обновить раздатки на Диске";

        private readonly IEnumerable<string> _sources;
        private readonly string _targetId;
        private readonly DataManager _googleDataManager;

        public UpdateCommand(IEnumerable<string> sources, string targetId, DataManager googleDataManager)
        {
            _sources = sources;
            _targetId = targetId;
            _googleDataManager = googleDataManager;
        }

        internal override Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            return Utils.UpdateAsync(message.Chat, client, _googleDataManager, _sources, _targetId);
        }
    }
}
