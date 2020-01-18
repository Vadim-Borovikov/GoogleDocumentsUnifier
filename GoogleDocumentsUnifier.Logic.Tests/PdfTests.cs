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
                Assert.AreEqual(PagesAmount, pdf.GetPagesAmount());
            }
        }

        [TestMethod]
        public void CreateWriterAndAddAllPagesTest()
        {
            using (var temp = new TempFile())
            {
                using (Pdf pdf = Pdf.CreateWriter(temp.File.FullName))
                {
                    using (Pdf other = Pdf.CreateReader(TestsConfiguration.Instance.Pdf2Path))
                    {
                        pdf.AddAllPages(other);
                        Assert.AreEqual(PagesAmount, pdf.GetPagesAmount());
                    }
                }
            }
        }

        private const int PagesAmount = 2;
    }
}