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
            var pdf = new Pdf();
            Assert.IsNotNull(pdf);
        }

        [TestMethod]
        public void PdfDocumentStreamTest()
        {
            using (var stream = new FileStream(InputTempPath, FileMode.Open))
            {
                var pdf = new Pdf(stream);
                Assert.IsNotNull(pdf);
                Assert.AreEqual(2, pdf.PagesAmount);
            }
        }

        [TestMethod]
        public void AddEmptyPageTest()
        {
            using (var stream = new FileStream(InputTempPath, FileMode.Open))
            {
                var pdf = new Pdf(stream);
                pdf.AddEmptyPage();
                Assert.IsNotNull(pdf);
                Assert.AreEqual(3, pdf.PagesAmount);
            }
        }

        [TestMethod]
        public void AddAllPagesTest()
        {
            var pdf = new Pdf();
            using (var stream = new FileStream(InputTempPath, FileMode.Open))
            {
                var other = new Pdf(stream);
                pdf.AddAllPages(other);
            }
            Assert.IsNotNull(pdf);
            Assert.AreEqual(2, pdf.PagesAmount);
        }

        [TestMethod]
        public void SaveTest()
        {
            var pdf = new Pdf();
            pdf.Save(OutputTempPath);
            Assert.IsTrue(File.Exists(OutputTempPath));
        }

        private const string InputTempPath = "D:/Test/pdf.pdf";
        private const string OutputTempPath = "D:/Test/test.pdf";
    }
}