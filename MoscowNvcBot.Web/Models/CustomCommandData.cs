using System.Collections.Generic;

namespace MoscowNvcBot.Web.Models
{
    internal class CustomCommandData
    {
        public readonly Dictionary<string, GoogleFileData> Files = new Dictionary<string, GoogleFileData>();
    }
}
