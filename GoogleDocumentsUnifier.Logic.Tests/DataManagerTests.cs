using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.Logic.Tests
{
    [TestClass]
    public class DataManagerTests
    {
        [TestMethod]
        public void UnifyTest()
        {
            string projectJson = File.ReadAllText(ProjectJsonPath);
            using (var dataManager = new DataManager(projectJson))
            {
                var pdf1 = new DocumentInfo(Pdf1Path, DocumentType.LocalPdf);
                var pdf2 = new DocumentInfo(Pdf2Path, DocumentType.LocalPdf);
                var requests = new[]
                {
                    new DocumentRequest(pdf1),
                    new DocumentRequest(pdf2)
                };
                using (var temp = new TempFile())
                {
                    dataManager.Unify(requests, temp.File.FullName);
                    using (Pdf pdf = Pdf.CreateReader(temp.File.FullName))
                    {
                        Assert.AreEqual(TotalPagesAmount, pdf.GetPagesAmount());
                    }
                }
            }
        }

        private const string ProjectJsonPath = "Keys/project.json";

        private const string Pdf1Path = "Test/pdf1.pdf";
        private const string Pdf2Path = "Test/pdf2.pdf";

        private const int TotalPagesAmount = 3;
    }
}