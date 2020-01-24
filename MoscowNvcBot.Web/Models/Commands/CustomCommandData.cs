using System.Collections.Generic;
using GoogleDocumentsUnifier.Logic;

namespace MoscowNvcBot.Web.Models.Commands
{
    public class CustomCommandData
    {
        public readonly Dictionary<string, DocumentRequest> Documents = new Dictionary<string, DocumentRequest>();
    }
}
