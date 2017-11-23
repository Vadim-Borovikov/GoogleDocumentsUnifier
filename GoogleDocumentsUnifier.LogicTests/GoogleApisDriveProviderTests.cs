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
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string credentialPath = Path.Combine(folderPath, ".credentials/drive-dotnet-quickstart.json");

                return new GoogleApisDriveProvider(stream, credentialPath, "user", CancellationToken.None);
            }
        }

        private const string PdfId = "0B3eXnAACJCqlWmlvWExrQjcyZFE";
        private const string DocId = "1lobinc-AwI3TEV9civH4_GyTPvIaZ5aBCGTspj67eNQ";
    }
}