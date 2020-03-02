﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Google;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoscowNvcBot.Web.Models.Commands
{
    public abstract class Command
    {
        internal abstract string Name { get; }
        internal abstract string Description { get; }

        internal bool Contains(Message message) => (message.Type == MessageType.Text) && message.Text.Contains(Name);

        internal virtual bool AdminsOnly => false;

        internal Task ExecuteAsyncWrapper(Message message, ITelegramBotClient client, bool fromAdmin)
        {
            CheckAccess(message.From, fromAdmin);
            return ExecuteAsync(message, client, fromAdmin);
        }

        internal Task InvokeAsyncWrapper(Message message, ITelegramBotClient client, string data, bool fromAdmin)
        {
            CheckAccess(message.From, fromAdmin);
            return InvokeAsync(message, client, data);
        }

        internal virtual Task HandleExceptionAsync(Exception exception, long chatId, ITelegramBotClient client)
        {
            if (!IsUsageLimitExceed(exception))
            {
                throw exception;
            }

            return HandleUsageLimitExcessAsync(chatId, client);
        }

        protected abstract Task ExecuteAsync(Message message, ITelegramBotClient client, bool fromAdmin);

        protected virtual Task InvokeAsync(Message message, ITelegramBotClient client, string data)
        {
            return Task.CompletedTask;
        }

        private static bool IsUsageLimitExceed(Exception exception)
        {
            return exception is GoogleApiException googleException &&
                (googleException.Error.Code == UsageLimitsExceededCode) &&
                googleException.Error.Errors.Any(e => e.Domain == UsageLimitsExceededDomain);
        }

        private static Task<Message> HandleUsageLimitExcessAsync(long chatId, ITelegramBotClient client)
        {
            return client.SendTextMessageAsync(chatId,
                "Google хочет отдохнуть от меня какое-то время. Попробуй позже, пожалуйста!");
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void CheckAccess(User user, bool isAdmin)
        {
            if (AdminsOnly && !isAdmin)
            {
                throw new Exception($"User @{user} is not in admin list!");
            }
        }

        private const int UsageLimitsExceededCode = 403;
        private const string UsageLimitsExceededDomain = "usageLimits";
    }
}
