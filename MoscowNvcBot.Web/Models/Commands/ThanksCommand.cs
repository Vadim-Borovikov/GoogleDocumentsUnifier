using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class ThanksCommand : Command
    {
        internal override string Name => "thanks";
        internal override string Description => "поблагодарить ведущих";

        internal override AccessType Type => AccessType.Users;

        public ThanksCommand(List<BotConfiguration.Payee> payees)
        {
            _payees = payees;
        }

        protected override async Task ExecuteAsync(Message message, ITelegramBotClient client, bool _)
        {
            foreach (BotConfiguration.Payee payee in _payees)
            {
                await Utils.SendMessage(payee, message.Chat, client);
            }
        }

        private readonly List<BotConfiguration.Payee> _payees;
    }
}
