namespace MoscowNvcBot.Web.Models
{
    internal static class AppSettings
    {
        private const string UrlBase = "https://moscownvcbot.azurewebsites.net";

        private const int Port = 443;

        public const string Route = "api/message/update";

        public static readonly string Url = $"{UrlBase}:{Port}/{Route}";

        public const string Key = "";
    }
}