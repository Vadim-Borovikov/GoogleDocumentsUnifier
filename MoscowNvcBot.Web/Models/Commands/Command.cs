using System;
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

        internal abstract Task ExecuteAsync(Message message, ITelegramBotClient client);

        internal virtual Task InvokeAsync(Message message, ITelegramBotClient client, string data)
        {
            return Task.CompletedTask;
        }

        internal virtual async Task HandleExceptionAsync(Exception exception, long chatId,
            ITelegramBotClient client)
        {
            if (!IsUsageLimitExceed(exception))
            {
                throw exception;
            }

            await HandleUsageLimitExcess(chatId, client);
        }

        private static bool IsUsageLimitExceed(Exception exception)
        {
            return exception is GoogleApiException googleException &&
                (googleException.Error.Code == UsageLimitsExceededCode) &&
                googleException.Error.Errors.Any(e => e.Domain == UsageLimitsExceededDomain);
        }

        private static async Task HandleUsageLimitExcess(long chatId, ITelegramBotClient client)
        {
            await client.SendTextMessageAsync(chatId,
                "Google хочет отдохнуть от меня какое-то время. Попробуй позже, пожалуйста!");
        }

        private const int UsageLimitsExceededCode = 403;
        private const string UsageLimitsExceededDomain = "usageLimits";
    }
}
