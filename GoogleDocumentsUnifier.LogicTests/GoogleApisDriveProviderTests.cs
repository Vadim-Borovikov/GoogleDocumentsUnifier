using System;
using System.IO;
using System.Threading;
using GoogleDocumentsUnifier.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.LogicTests
{
    [TestClass]
    public class GoogleApisDriveProviderTests
    {
        [TestMethod]
        public void DownloadFileTest()
        {
            var file = new FileInfo("D:/Test/pdf.pdf");
            using (GoogleApisDriveProvider provider = CreateProvider())
            {
                using (var stream = new FileStream(file.FullName, FileMode.Create))
                {
                    provider.DownloadFile(PdfId, stream);
                }
            }
            Assert.IsTrue(file.Exists);
            Assert.AreNotEqual(0, file.Length);
        }

        [TestMethod]
        public void ExportFileTest()
        {
            var file = new FileInfo("D:/Test/doc.pdf");
            using (GoogleApisDriveProvider provider = CreateProvider())
            {
                using (var stream = new FileStream(file.FullName, FileMode.Create))
                {
                    provider.ExportFile(DocId, "application/pdf", stream);
                }
            }
            Assert.IsTrue(file.Exists);
            Assert.AreNotEqual(0, file.Length);
        }

        private static GoogleApisDriveProvider CreateProvider()
        {
            using (var stream = new FileStream("Keys/project.json", FileMode.Open, FileAccess.Read))
            {
                return new GoogleApisDriveProvider(stream);
            }
        }

        private const string PdfId = "17hnk4p5kIS8U4vK5JB18B59WMy-dEVvk";
        private const string DocId = "1WxXjtQu03JfLR5dhECNWcYX8EyoHUJePDrUocQnsB8g";
    }
}