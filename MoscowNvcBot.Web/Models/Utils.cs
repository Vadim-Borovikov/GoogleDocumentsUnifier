﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoscowNvcBot.Web.Models
{
    internal static class Utils
    {
        internal static async Task<Message> FinalizeStatusMessageAsync(Message message, ITelegramBotClient client,
            string postfix = "")
        {
            return await client.EditMessageTextAsync(message.Chat, message.MessageId,
                $"_{message.Text}_ Готово.{postfix}", ParseMode.Markdown);
        }

        internal static async Task UpdateAsync(Chat chat, ITelegramBotClient client, DataManager googleDataManager,
            IEnumerable<string> sourceIds, string parent,
            Func<string, DataManager, string, Task<PdfData>> pdfAsyncChecker,
            Func<PdfData, DataManager, string, Task> pdfAsyncCreater)
        {
            Message checkingMessage = await client.SendTextMessageAsync(chat, "_Проверяю…_", ParseMode.Markdown);

            List<Task<PdfData>> checkTasks =
                sourceIds.Select(id => pdfAsyncChecker(id, googleDataManager, parent)).ToList();
            PdfData[] datas = await Task.WhenAll(checkTasks);

            List<PdfData> filesToUpdate = datas.Where(d => d.Status != PdfData.FileStatus.Ok).ToList();

            if (filesToUpdate.Any())
            {
                await FinalizeStatusMessageAsync(checkingMessage, client);

                Message updatingMessage = await client.SendTextMessageAsync(chat, "_Обновляю…_", ParseMode.Markdown);

                List<Task> updateTasks =
                    filesToUpdate.Select(f => pdfAsyncCreater(f, googleDataManager, parent)).ToList();
                await Task.WhenAll(updateTasks);

                await FinalizeStatusMessageAsync(updatingMessage, client);
            }
            else
            {
                await FinalizeStatusMessageAsync(checkingMessage, client, " Раздатки уже актуальны.");
            }
        }
    }
}
