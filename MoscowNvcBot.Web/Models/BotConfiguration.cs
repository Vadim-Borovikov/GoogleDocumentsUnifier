using System.Collections.Generic;

namespace MoscowNvcBot.Web.Models
{
    public class BotConfiguration
    {
        public string Token { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string StartMessagePrefix { get; set; }

        public string GoogleProjectJson { get; set; }

        public List<string> Sources { get; set; }

        public const string Route = "api/update";

        public string Url => $"{Host}:{Port}/{Route}";
    }
}