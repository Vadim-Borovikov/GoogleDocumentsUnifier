using System.Collections.Generic;

namespace MoscowNvcBot.Web.Models.Commands
{
    internal class CustomCommandData
    {
        public readonly Dictionary<string, CustomCommandFileData> Files =
            new Dictionary<string, CustomCommandFileData>();
    }
}
