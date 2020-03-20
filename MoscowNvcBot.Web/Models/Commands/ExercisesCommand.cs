﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class ExercisesCommand : Command
    {
        internal override string Name => "exercises";
        internal override string Description => "упражнения";

        public ExercisesCommand(string template, IEnumerable<string> links)
        {
            _template = template;
            _links = links;
        }

        protected override async Task ExecuteAsync(Message message, ITelegramBotClient client, bool _)
        {
            foreach (string text in _links.Select(l => string.Format(_template, l)))
            {
                await client.SendTextMessageAsync(message.Chat, text, ParseMode.Html);
            }
        }

        private readonly string _template;
        private readonly IEnumerable<string> _links;
    }
}