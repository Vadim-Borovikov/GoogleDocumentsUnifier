using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.Logic.Tests
{
    [TestClass]
    internal class TestsConfiguration
    {
        public static TestsConfiguration Instance;

        public readonly string Pdf1Path;
        public readonly string Pdf2Path;
        public readonly string PdfId;
        public readonly string DocId;
        public readonly string GoogleProjectJson;

        public TestsConfiguration(string pdf1Path, string pdf2Path, string pdfId, string docId,
            string googleProjectJson)
        {
            Pdf1Path = pdf1Path;
            Pdf2Path = pdf2Path;
            PdfId = pdfId;
            DocId = docId;
            GoogleProjectJson = googleProjectJson;
        }

        [AssemblyInitialize]
        public static void Init(TestContext testContext)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", true)
                .Build();

            string googleProjectJson = testContext.Properties["GoogleProjectJson"]?.ToString()?.Replace("__", " ");
            if (string.IsNullOrWhiteSpace(googleProjectJson))
            {
                googleProjectJson = config["GoogleProjectJson"];
            }

            Instance = new TestsConfiguration(config["Pdf1Path"], config["Pdf2Path"], config["PdfId"], config["DocId"],
                googleProjectJson);
        }
    }
}