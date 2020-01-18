using System.IO;
using System.Threading.Tasks;
using Google.Apis.Download;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.Logic.Tests
{
    [TestClass]
    public class GoogleApisDriveProviderTests
    {
        [TestMethod]
        public async Task DownloadFileTest()
        {
            using (var temp = new TempFile())
            {
                using (GoogleApisDriveProvider provider = CreateProvider())
                {
                    using (var stream = new FileStream(temp.File.FullName, FileMode.Open))
                    {
                        IDownloadProgress progress =
                            await provider.DownloadFileAsync(TestsConfiguration.Instance.PdfId, stream);
                        Assert.AreEqual(DownloadStatus.Completed, progress.Status);
                    }
                }
                CheckFile(temp.File);
            }
        }

        [TestMethod]
        public async Task ExportFileTest()
        {
            using (var temp = new TempFile())
            {
                using (GoogleApisDriveProvider provider = CreateProvider())
                {
                    using (var stream = new FileStream(temp.File.FullName, FileMode.Open))
                    {
                        IDownloadProgress progress =
                            await provider.ExportFileAsync(TestsConfiguration.Instance.DocId, "application/pdf", stream);
                        Assert.AreEqual(DownloadStatus.Completed, progress.Status);
                    }
                }
                CheckFile(temp.File);
            }
        }

        private static GoogleApisDriveProvider CreateProvider()
        {
            return new GoogleApisDriveProvider(TestsConfiguration.Instance.GoogleProjectJson);
        }

        private static void CheckFile(System.IO.FileInfo file)
        {
            Assert.IsTrue(file.Exists);
            Assert.AreNotEqual(0, file.Length);
        }
    }
}