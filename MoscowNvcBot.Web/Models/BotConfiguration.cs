using System.Collections.Generic;

namespace MoscowNvcBot.Web.Models
{
    public class BotConfiguration
    {
        public string Token { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string GoogleProjectJson { get; set; }

        public List<string> DocumentIds { get; set; }

        public string FolderId { get; set; }

        public string LocalPath { get; set; }

        public string Url => $"{Host}:{Port}/{Token}";

        public List<string> CheckListLines { get; set; }

        public string CheckList => string.Join('\n', CheckListLines);
    }
}