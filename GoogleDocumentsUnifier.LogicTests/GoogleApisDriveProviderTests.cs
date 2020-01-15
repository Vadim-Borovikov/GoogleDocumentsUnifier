using System.IO;
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
            var file = new FileInfo(OutputPdfPath);
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
            var file = new FileInfo(OutputDocPath);
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
            string projectJson = File.ReadAllText(ProjectJsonPath);
            return new GoogleApisDriveProvider(projectJson);
        }

        private const string ProjectJsonPath = "Keys/project.json";
        private const string OutputPdfPath = "Test/pdf.pdf";
        private const string OutputDocPath = "Test/doc.pdf";
        private const string PdfId = "17hnk4p5kIS8U4vK5JB18B59WMy-dEVvk";
        private const string DocId = "1WxXjtQu03JfLR5dhECNWcYX8EyoHUJePDrUocQnsB8g";
    }
}