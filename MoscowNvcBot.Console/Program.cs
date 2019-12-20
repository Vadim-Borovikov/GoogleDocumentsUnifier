using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using GoogleDocumentsUnifier.Logic;
using Telegram.Bot.Types;

namespace MoscowNvcBot.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string clientSecretPath = ConfigurationManager.AppSettings.Get("clientSecretPath");

            string token = ConfigurationManager.AppSettings.Get("token");
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            string sourcesSetting = ConfigurationManager.AppSettings.Get("sources");
            string[] sources = sourcesSetting.Split(new [] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            List<DocumentInfo> infos = sources.Select(CreateInfo).ToList();

            using (var googleDataManager = new DataManager(clientSecretPath))
            {
                var botLogic = new MoscowNvcBotLogic(token, infos, googleDataManager);

                User me = botLogic.Bot.GetMeAsync().Result;
                System.Console.Title = me.Username;

                botLogic.Bot.StartReceiving();
                System.Console.WriteLine($"Start listening for @{me.Username}");
                System.Console.ReadLine();
                botLogic.Bot.StopReceiving();
            }
        }

        private static DocumentInfo CreateInfo(string source)
        {
            string id = new string(source.Where(c => !char.IsWhiteSpace(c)).ToArray());
            return new DocumentInfo(id, DocumentType.GoogleDocument);
        }
    }
}
    