using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using GoogleDocumentsUnifier.Logic;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace MoscowNvcBot.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string googleClientSecretPath = ConfigurationManager.AppSettings.Get("googleClientSecretPath");
            if (!File.Exists(googleClientSecretPath))
            {
                System.Console.WriteLine($"No {googleClientSecretPath} found!");
                return;
            }

            string tokenPath = ConfigurationManager.AppSettings.Get("telegramTokenPath");
            if (!File.Exists(tokenPath))
            {
                System.Console.WriteLine($"No {tokenPath} found!");
                return;
            }
            string telegramToken = File.ReadAllText(tokenPath);
            if (string.IsNullOrWhiteSpace(telegramToken))
            {
                System.Console.WriteLine($"Token in {tokenPath} in empty!");
                return;
            }

            string[] sources = GetArraySetting("sources");
            List<DocumentInfo> infos = sources.Select(CreateGoogleDocumentInfo).ToList();

            using (var googleDataManager = new DataManager(googleClientSecretPath))
            {
                var botLogic = new MoscowNvcBotLogic(telegramToken, infos, googleDataManager);

                User me = botLogic.Bot.GetMeAsync().Result;
                System.Console.Title = me.Username;

                botLogic.Bot.StartReceiving();
                System.Console.WriteLine($"Start listening for @{me.Username}");
                System.Console.ReadLine();
                botLogic.Bot.StopReceiving();
            }
        }

        private static string[] GetArraySetting(string name)
        {
            string setting = ConfigurationManager.AppSettings.Get(name);
            char[] separator = { ';' };
            string[] array = setting.Split(separator);
            return RemoveWhiteSpace(array).ToArray();
        }

        private static IEnumerable<string> RemoveWhiteSpace(IEnumerable<string> strings)
        {
            return strings.Select(RemoveWhiteSpace).Where(s => !string.IsNullOrWhiteSpace(s));
        }

        private static string RemoveWhiteSpace(string s)
        {
            return new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        private static DocumentInfo CreateGoogleDocumentInfo(string id)
        {
            return new DocumentInfo(id, DocumentType.GoogleDocument);
        }
    }
}
