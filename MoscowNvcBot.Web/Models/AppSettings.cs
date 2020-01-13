using System;

namespace MoscowNvcBot.Web.Models
{
    public static class AppSettings
    {
        public static string Url { get; set; } = "https://moscownvcbot.azurewebsites.net:443/{0}";
        public static string Name { get; set; } = "moscow_nvc_bot";
        public static string Key { get; set; } = "";
    }
}