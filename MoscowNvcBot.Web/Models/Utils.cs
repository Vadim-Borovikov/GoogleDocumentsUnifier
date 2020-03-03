﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoogleDocumentsUnifier.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
using FileInfo = GoogleDocumentsUnifier.Logic.FileInfo;

namespace MoscowNvcBot.Web.Models
{
    internal static class Utils
    {
        internal static async Task<Message> FinalizeStatusMessageAsync(Message message, ITelegramBotClient client,
            string postfix = "")
        {
            Chat chat = message.Chat;
            string text = $"_{message.Text}_ Готово.{postfix}";
            await client.DeleteMessageAsync(chat, message.MessageId);
            return await client.SendTextMessageAsync(chat, text, ParseMode.Markdown);
        }

        internal static async Task<List<PdfData>> CheckAsync(IEnumerable<string> sources,
            Func<string, Task<PdfData>> check)
        {
            PdfData[] datas = await Task.WhenAll(sources.Select(check));
            return datas.Where(d => d.Status != PdfData.FileStatus.Ok).ToList();
        }

        internal static async Task CreateOrUpdateAsync(IEnumerable<PdfData> sources,
            Func<PdfData, Task> createOrUpdate)
        {
            List<Task> updateTasks = sources.Select(createOrUpdate).ToList();
            await Task.WhenAll(updateTasks);
        }

        internal static async Task<PdfData> CheckLocalPdfAsync(string sourceId, DataManager googleDataManager,
            string pdfFolderPath)
        {
            FileInfo fileInfo = await googleDataManager.GetFileInfoAsync(sourceId);

            string path = Path.Combine(pdfFolderPath, $"{fileInfo.Name}.pdf");
            if (!File.Exists(path))
            {
                return PdfData.CreateNoneLocal(sourceId, path);
            }

            if (File.GetLastWriteTime(path) < fileInfo.ModifiedTime)
            {
                return PdfData.CreateOutdatedLocal(sourceId, path);
            }

            return PdfData.CreateOk();
        }

        internal static async Task CreateOrUpdateLocalAsync(PdfData data, DataManager googleDataManager,
            string pdfFolderPath)
        {
            var info = new DocumentInfo(data.SourceId, DocumentType.Document);
            string path = Path.Combine(pdfFolderPath, data.Name);
            switch (data.Status)
            {
                case PdfData.FileStatus.None:
                case PdfData.FileStatus.Outdated:
                    await googleDataManager.DownloadAsync(info, path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(data.Status), data.Status, "Unexpected Pdf status!");
            }
        }

        internal static Task SendMessage(BotConfiguration.Link link, Chat chat, ITelegramBotClient client)
        {
            if (link.MakeButton)
            {
                InlineKeyboardMarkup keyboard = GetReplyMarkup(link);
                return
                    client.SendTextMessageAsync(chat, link.Name, replyMarkup: keyboard, disableWebPagePreview: true);
            }

            string text = $"[{link.Name}]({link.Url})";
            return client.SendTextMessageAsync(chat, text, ParseMode.Markdown);
        }

        internal static async Task SendMessage(BotConfiguration.Payee payee,
            IReadOnlyDictionary<string, BotConfiguration.Link> banks, Chat chat, ITelegramBotClient client)
        {
            using (var stream = new FileStream(payee.PhotoPath, FileMode.Open))
            {
                var photo = new InputOnlineFile(stream);
                string caption = GetCaption(payee.Name, payee.Accounts, banks);
                await client.SendPhotoAsync(chat, photo, caption, ParseMode.Markdown);
            }
        }
        private static InlineKeyboardMarkup GetReplyMarkup(BotConfiguration.Link link)
        {
            var button = new InlineKeyboardButton
            {
                Text = "Открыть",
                Url = link.Url
            };
            return new InlineKeyboardMarkup(button);
        }

        private static string GetCaption(string name, IEnumerable<BotConfiguration.Payee.Account> accounts,
            IReadOnlyDictionary<string, BotConfiguration.Link> banks)
        {
            IEnumerable<string> texts = accounts.Select(a => GetText(a, banks[a.BankId]));
            string options = string.Join($" или{Environment.NewLine}", texts);
            return $"{name}:{Environment.NewLine}{options}";
        }

        private static string GetText(BotConfiguration.Payee.Account account, BotConfiguration.Link bank)
        {
            return $"`{account.CardNumber}` в [{bank.Name}]({bank.Url})";
        }
    }
}
