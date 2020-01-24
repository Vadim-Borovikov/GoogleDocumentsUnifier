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
        public readonly int Pdf1Pages;
        public readonly int Pdf2Pages;
        public readonly string PdfParentId;
        public readonly int PdfParentChildrenAmount;
        public readonly string PdfId;
        public readonly string PdfName;
        public readonly string UpdatablePdfId;
        public readonly string GoogleProjectJson;

        public TestsConfiguration(string pdf1Path, string pdf2Path, int pdf1Pages, int pdf2Pages, string pdfParentId,
            int pdfParentChildrenAmount, string pdfId, string pdfName, string updatablePdfId, string googleProjectJson)
        {
            Pdf1Path = pdf1Path;
            Pdf2Path = pdf2Path;
            Pdf1Pages = pdf1Pages;
            Pdf2Pages = pdf2Pages;
            PdfParentId = pdfParentId;
            PdfParentChildrenAmount = pdfParentChildrenAmount;
            PdfId = pdfId;
            PdfName = pdfName;
            UpdatablePdfId = updatablePdfId;
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

            Instance = new TestsConfiguration(config["Pdf1Path"], config["Pdf2Path"], int.Parse(config["Pdf1Pages"]),
                int.Parse(config["Pdf2Pages"]), config["PdfParentId"], int.Parse(config["PdfParentChildrenAmount"]),
                config["PdfId"], config["PdfName"], config["UpdatablePdfId"], googleProjectJson);
        }
    }
}