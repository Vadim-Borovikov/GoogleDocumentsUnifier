using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.Logic.Tests
{
    [TestClass]
    public class DataManagerTests
    {
        [TestMethod]
        public async Task GetFileInfoTest()
        {
            using (DataManager dataManager = CreateDataManager())
            {
                FileInfo info = await dataManager.GetFileInfoAsync(TestsConfiguration.Instance.PdfId);
                CheckPdfInfo(info);
            }
        }

        [TestMethod]
        public async Task FindFileInFolderTest()
        {
            using (DataManager dataManager = CreateDataManager())
            {
                FileInfo info = await dataManager.FindFileInFolderAsync(TestsConfiguration.Instance.PdfParentId,
                    TestsConfiguration.Instance.PdfName);
                CheckPdfInfo(info);
            }
        }

        [TestMethod]
        public async Task GetFilesInFolderTest()
        {
            using (DataManager dataManager = CreateDataManager())
            {
                IEnumerable<FileInfo> infos =
                    await dataManager.GetFilesInFolderAsync(TestsConfiguration.Instance.PdfParentId);
                Assert.AreEqual(TestsConfiguration.Instance.PdfParentChildrenAmount, infos.Count());
            }
        }

        [TestMethod]
        public void UnifyTest()
        {
            var requests = new[]
            {
                new DocumentRequest(TestsConfiguration.Instance.Pdf1Path, 1),
                new DocumentRequest(TestsConfiguration.Instance.Pdf2Path, 1)
            };
            int pages = TestsConfiguration.Instance.Pdf1Pages + TestsConfiguration.Instance.Pdf2Pages;
            using (TempFile temp = DataManager.Unify(requests))
            {
                using (Pdf pdf = Pdf.CreateReader(temp.Path))
                {
                    Assert.AreEqual(pages, pdf.GetPagesAmount());
                }
            }
        }

        [TestMethod]
        public async Task UpdateAndDownloadTest()
        {
            using (DataManager dataManager = CreateDataManager())
            {
                await dataManager.UpdateAsync(TestsConfiguration.Instance.UpdatablePdfId,
                    TestsConfiguration.Instance.Pdf1Path);

                await CheckGooglePdfWithTemp(dataManager, TestsConfiguration.Instance.UpdatablePdfId,
                    TestsConfiguration.Instance.Pdf1Pages);

                await dataManager.UpdateAsync(TestsConfiguration.Instance.UpdatablePdfId,
                    TestsConfiguration.Instance.Pdf2Path);

                await CheckGooglePdf(dataManager, TestsConfiguration.Instance.UpdatablePdfId,
                    TestsConfiguration.Instance.Pdf2Pages);
            }
        }

        private static DataManager CreateDataManager()
        {
            return new DataManager(TestsConfiguration.Instance.GoogleProjectJson);
        }

        private static void CheckPdfInfo(FileInfo info)
        {
            Assert.IsNotNull(info);
            Assert.AreEqual(TestsConfiguration.Instance.PdfId, info.Id);
            Assert.AreEqual(TestsConfiguration.Instance.PdfName, info.Name);
        }

        private static async Task CheckGooglePdfWithTemp(DataManager dataManager, string id, int pages)
        {
            var info = new DocumentInfo(id, DocumentType.Pdf);
            using (TempFile temp = await dataManager.DownloadAsync(info))
            {
                CheckLocalPdf(temp.Path, pages);
            }
        }

        private static async Task CheckGooglePdf(DataManager dataManager, string id, int pages)
        {
            var info = new DocumentInfo(id, DocumentType.Pdf);
            using (var temp = new TempFile())
            {
                await dataManager.DownloadAsync(info, temp.Path);
                CheckLocalPdf(temp.Path, pages);
            }
        }

        private static void CheckLocalPdf(string path, int pages)
        {
            Assert.IsTrue(File.Exists(path));
            string content = File.ReadAllText(path);
            Assert.AreNotEqual(0, content.Length);
            using (Pdf pdf = Pdf.CreateReader(path))
            {
                Assert.AreEqual(pages, pdf.GetPagesAmount());
            }
        }
    }
}