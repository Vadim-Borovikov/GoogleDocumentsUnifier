using System.Configuration;
using GoogleDocumentsUnifier.Logic;

namespace GoogleDocumentsUnifier
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string clientSecretPath = ConfigurationManager.AppSettings.Get("clientSecretPath");

            string[] docIds = ConfigurationManager.AppSettings.Get("docIds").Split(';');
            string[] pdfIds = ConfigurationManager.AppSettings.Get("pdfIds").Split(';');

            var thesises = new DocumentInfo(docIds[0], false);
            var feelings = new DocumentInfo(pdfIds[0], true);
            var needs = new DocumentInfo(docIds[1], false);

            using (var dataManager = new DataManager(clientSecretPath))
            {
                var requests = new[]
                {
                    new DocumentRequest(thesises, 10),
                    new DocumentRequest(feelings, 10),
                    new DocumentRequest(needs, 10)
                };
                dataManager.Unify(requests, "D:/Test/result.pdf");
            }
        }
    }
}
