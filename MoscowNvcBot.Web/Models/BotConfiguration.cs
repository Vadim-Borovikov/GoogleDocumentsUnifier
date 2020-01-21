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

        public string TargetId { get; set; }

        public string TargetPrefix { get; set; }

        public string Url => $"{Host}:{Port}";
    }
}