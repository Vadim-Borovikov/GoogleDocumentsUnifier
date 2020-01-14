namespace MoscowNvcBot.Web.Models
{
    internal static class Configuration
    {
        private const string Host = "https://moscownvcbot.azurewebsites.net";

        private const int Port = 443;

        public const string Route = "api/update";

        public static readonly string Url = $"{Host}:{Port}/{Route}";

        public const string Token = "";
    }
}