using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.Logic.Tests
{
    [TestClass]
    public class PdfTests
    {
        [TestMethod]
        public void PdfFileReaderAndPagesAmountTest()
        {
            using (Pdf pdf = Pdf.CreateReader(TestsConfiguration.Instance.Pdf2Path))
            {
                Assert.IsNotNull(pdf);
                Assert.AreEqual(TestsConfiguration.Instance.Pdf2Pages, pdf.GetPagesAmount());
            }
        }

        [TestMethod]
        public void CreateWriterAndAddAllPagesTest()
        {
            using (var temp = new TempFile())
            {
                using (Pdf pdf = Pdf.CreateWriter(temp.Path))
                {
                    using (Pdf other = Pdf.CreateReader(TestsConfiguration.Instance.Pdf2Path))
                    {
                        pdf.AddAllPages(other);
                        Assert.AreEqual(TestsConfiguration.Instance.Pdf2Pages, pdf.GetPagesAmount());
                    }
                }
            }
        }
    }
}