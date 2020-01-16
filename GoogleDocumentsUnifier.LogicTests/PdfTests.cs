using System.IO;
using GoogleDocumentsUnifier.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.LogicTests
{
    [TestClass]
    public class PdfTests
    {
        [TestMethod]
        public void PdfTest()
        {
            using (var pdf = new Pdf())
            {
                Assert.IsNotNull(pdf);
            }
        }

        [TestMethod]
        public void PdfFileAndPagesAmountTest()
        {
            using (var pdf = new Pdf(Path))
            {
                Assert.IsNotNull(pdf);
                Assert.AreEqual(PagesAmount, pdf.PagesAmount);
            }
        }

        [TestMethod]
        public void PdfFileStreamTest()
        {
            using (var stream = new FileStream(Path, FileMode.Open))
            {
                using (var pdf = new Pdf(stream))
                {
                    Assert.IsNotNull(pdf);
                    Assert.AreEqual(PagesAmount, pdf.PagesAmount);
                }
            }
        }

        [TestMethod]
        public void AddAllPagesTest()
        {
            using (var pdf = new Pdf())
            {
                using (var other = new Pdf(Path))
                {
                    pdf.AddAllPages(other);
                    Assert.AreEqual(PagesAmount, pdf.PagesAmount);
                }
            }
        }

        [TestMethod]
        public void SaveTest()
        {
            using (var pdf = new Pdf(Path))
            {
                using (var temp = new TempFile())
                {
                    pdf.Save(temp.File.FullName);
                    Assert.IsTrue(temp.File.Exists);
                    using (var tempPdf = new Pdf(temp.File.FullName))
                    {
                        Assert.IsNotNull(tempPdf);
                        Assert.AreEqual(PagesAmount, tempPdf.PagesAmount);
                    }
                }
            }
        }

        private const string Path = "Test/pdf2.pdf";

        private const int PagesAmount = 2;
    }
}