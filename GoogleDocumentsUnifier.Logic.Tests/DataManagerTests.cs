using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.Logic.Tests
{
    [TestClass]
    public class DataManagerTests
    {
        [TestMethod]
        public async Task UnifyTest()
        {
            using (var dataManager = new DataManager(TestsConfiguration.Instance.GoogleProjectJson))
            {
                var pdf1 = new DocumentInfo(TestsConfiguration.Instance.Pdf1Path, DocumentType.LocalPdf);
                var pdf2 = new DocumentInfo(TestsConfiguration.Instance.Pdf2Path, DocumentType.LocalPdf);
                var requests = new[]
                {
                    new DocumentRequest(pdf1, 1),
                    new DocumentRequest(pdf2, 1)
                };
                using (var temp = new TempFile())
                {
                    await dataManager.UnifyAsync(requests, temp.File.FullName);
                    using (Pdf pdf = Pdf.CreateReader(temp.File.FullName))
                    {
                        Assert.AreEqual(TotalPagesAmount, pdf.GetPagesAmount());
                    }
                }
            }
        }

        private const int TotalPagesAmount = 3;
    }
}