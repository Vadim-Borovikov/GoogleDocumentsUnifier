using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.Logic.Tests
{
    [TestClass]
    public class PdfTests
    {
        [TestMethod]
        public void PdfFileReaderAndPagesAmountTest()
        {
            using (Pdf pdf = Pdf.CreateReader(Path))
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
                    using (Pdf other = Pdf.CreateReader(Path))
                    {
                        pdf.AddAllPages(other);
                        Assert.AreEqual(PagesAmount, pdf.GetPagesAmount());
                    }
                }
            }
        }

        private const string Path = "Test/pdf2.pdf";

        private const int PagesAmount = 2;
    }
}