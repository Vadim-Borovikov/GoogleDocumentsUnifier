// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global
using System;
using System.Collections.Generic;

namespace MoscowNvcBot.Web.Models
{
    public class BotConfiguration
    {
        public class Link
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public bool MakeButton { get; set; }
        }

        public string Token { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public int PingPeriodSeconds { get; set; }

        public TimeSpan PingPeriod => TimeSpan.FromSeconds(PingPeriodSeconds);

        public string GoogleProjectJson { get; set; }

        public List<string> DocumentIds { get; set; }

        public string PdfFolderId { get; set; }

        public string PdfFolderPath { get; set; }

        public string Url => $"{Host}:{Port}/{Token}";

        public List<string> CheckListLines { get; set; }

        public string CheckList => string.Join('\n', CheckListLines);

        public List<int> AdminIds { get; set; }

        public List<Link> Links { get; set; }

        public Link FeedbackLink { get; set; }
    }
}