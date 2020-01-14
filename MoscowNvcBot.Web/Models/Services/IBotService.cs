﻿using System.Collections.Generic;
using MoscowNvcBot.Web.Models.Commands;
using Telegram.Bot;

namespace MoscowNvcBot.Web.Models.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
        IReadOnlyList<Command> Commands { get; }
    }
}