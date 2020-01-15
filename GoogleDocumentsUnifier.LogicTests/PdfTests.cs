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
        public void PdfDocumentStreamTest()
        {
            PdfDocumentStreamTest(InputPdfPath, 13);
            PdfDocumentStreamTest(InputDocPath, 1);
        }

        [TestMethod]
        public void AddEmptyPageAndSaveTest()
        {
            using (var pdf = new Pdf())
            {
                pdf.AddEmptyPage();
                Assert.IsNotNull(pdf);
                Assert.AreEqual(1, pdf.PagesAmount);
                pdf.Save(OutputTempPath);
                Assert.IsTrue(File.Exists(OutputTempPath));
            }
        }

        [TestMethod]
        public void AddAllPagesTest()
        {
            using (var pdf = new Pdf())
            {
                using (var stream = new FileStream(InputPdfPath, FileMode.Open))
                {
                    using (var other = new Pdf(stream))
                    {
                        pdf.AddAllPages(other);
                        Assert.IsNotNull(pdf);
                        Assert.AreEqual(13, pdf.PagesAmount);
                    }
                }
            }
        }

        private static void PdfDocumentStreamTest(string path, int pagesExpected)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                using (var pdf = new Pdf(stream))
                {
                    Assert.IsNotNull(pdf);
                    Assert.AreEqual(pagesExpected, pdf.PagesAmount);
                }
            }
        }

        private const string InputPdfPath = "Test/pdf.pdf";
        private const string InputDocPath = "Test/doc.pdf";
        private const string OutputTempPath = "Test/test.pdf";
    }
}