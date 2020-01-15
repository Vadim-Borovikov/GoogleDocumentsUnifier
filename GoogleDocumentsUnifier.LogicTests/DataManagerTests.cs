using GoogleDocumentsUnifier.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.LogicTests
{
    [TestClass]
    public class DataManagerTests
    {
        [TestMethod]
        public void UnifyTest()
        {
            using (var dataManager = new DataManager("Keys/project.json"))
            {
                var pdf = new DocumentInfo(InputPdfPath, DocumentType.LocalPdf);
                var requests = new[]
                {
                    new DocumentRequest(pdf, 1)
                };
                dataManager.Unify(requests, OutputTempPath, false);
            }
        }

        private const string InputPdfPath = "D:/Test/pdf.pdf";
        private const string OutputTempPath = "D:/Test/result.pdf";
    }
}