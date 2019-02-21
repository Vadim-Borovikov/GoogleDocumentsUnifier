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

            string[] names =
            {
                "checklist",
                "landing",
                "case_manual",
                "case_template",
                "empathy_manual",
                "conflict_manual",
                "refusals_manual",
                "power_words_manual",
                "exercises",
                "feelings",
                "needs"
            };

            Dictionary<string, DocumentInfo> sources =
                names.ToDictionary(n => n,
                                   n => new DocumentInfo(ConfigurationManager.AppSettings.Get(n),
                                                         DocumentType.GoogleDocument));

            using (var googleDataManager = new DataManager(clientSecretPath))
            {
                var botLogic = new MoscowNvcBotLogc(token, sources, googleDataManager);

                User me = botLogic.Bot.GetMeAsync().Result;
                System.Console.Title = me.Username;

                botLogic.Bot.StartReceiving();
                System.Console.WriteLine($"Start listening for @{me.Username}");
                System.Console.ReadLine();
                botLogic.Bot.StopReceiving();
            }
        }
    }
}
    