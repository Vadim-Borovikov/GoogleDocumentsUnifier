using System;
using System.Configuration;
using System.IO;
using System.Threading;
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
            string checklistId = ConfigurationManager.AppSettings.Get("checklistId");
            string casesId = ConfigurationManager.AppSettings.Get("casesId");
            string empathyId = ConfigurationManager.AppSettings.Get("empathyId");

            using (var stream = new FileStream(clientSecretPath, FileMode.Open, FileAccess.Read))
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string credentialPath =
                    Path.Combine(folderPath, ".credentials/drive-dotnet-quickstart.json");

                using (var googleProvider =
                    new GoogleApisDriveProvider(stream, credentialPath, "user", CancellationToken.None))
                {
                    var botLogic =
                        new MoscowNvcBotLogc(token, checklistId, casesId, empathyId, googleProvider);

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
}
    