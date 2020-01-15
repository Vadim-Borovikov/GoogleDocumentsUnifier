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
        private static string _googleProjectJson;
        private static string _telegramToken;
        private static List<DocumentInfo> _infos;

        private static void Main(string[] args)
        {
            bool success = LoadConfig();
            if (!success)
            {
                return;
            }

            SetupBot();
        }

        private static bool LoadConfig()
        {
            string googleProjectPath = ConfigurationManager.AppSettings.Get("googleProjectPath");
            if (!File.Exists(googleProjectPath))
            {
                System.Console.WriteLine($"No {googleProjectPath} found!");
                return false;
            }
            _googleProjectJson = File.ReadAllText(googleProjectPath);

            string tokenPath = ConfigurationManager.AppSettings.Get("telegramTokenPath");
            if (!File.Exists(tokenPath))
            {
                System.Console.WriteLine($"No {tokenPath} found!");
                return false;
            }
            _telegramToken = File.ReadAllText(tokenPath);
            if (string.IsNullOrWhiteSpace(_telegramToken))
            {
                System.Console.WriteLine($"Token in {tokenPath} in empty!");
                return false;
            }

            string[] sources = GetArraySetting("sources");
            _infos = sources.Select(CreateGoogleDocumentInfo).ToList();

            return true;
        }

        private static void SetupBot()
        {
            using (var googleDataManager = new DataManager(_googleProjectJson))
            {
                var botLogic = new MoscowNvcBotLogic(_telegramToken, _infos, googleDataManager);

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
