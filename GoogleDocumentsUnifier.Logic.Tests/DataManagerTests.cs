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
        public async Task UnifyTest()
        {
            var pdf1 = new DocumentInfo(TestsConfiguration.Instance.Pdf1Path, DocumentType.LocalPdf);
            var pdf2 = new DocumentInfo(TestsConfiguration.Instance.Pdf2Path, DocumentType.LocalPdf);
            var requests = new[]
            {
                new DocumentRequest(pdf1, 1),
                new DocumentRequest(pdf2, 1)
            };
            int pages = TestsConfiguration.Instance.Pdf1Pages + TestsConfiguration.Instance.Pdf2Pages;
            using (DataManager dataManager = CreateDataManager())
            {
                using (var temp = new TempFile())
                {
                    await dataManager.UnifyAsync(requests, temp.File.FullName);
                    using (Pdf pdf = Pdf.CreateReader(temp.File.FullName))
                    {
                        Assert.AreEqual(pages, pdf.GetPagesAmount());
                    }
                }
            }
        }

        [TestMethod]
        public async Task UpdateAndCopyTest()
        {
            using (DataManager dataManager = CreateDataManager())
            {
                await dataManager.UpdateAsync(TestsConfiguration.Instance.UpdatablePdfId,
                    TestsConfiguration.Instance.Pdf1Path);

                await CheckGooglePdf(dataManager, TestsConfiguration.Instance.UpdatablePdfId,
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

        private static async Task CheckGooglePdf(DataManager dataManager, string id, int pages)
        {
            var info = new DocumentInfo(id, DocumentType.GooglePdf);
            var request = new DocumentRequest(info, 1);
            using (var temp = new TempFile())
            {
                await dataManager.CopyAsync(request, temp.File.FullName);
                CheckLocalPdf(temp.File, pages);
            }
        }

        private static void CheckLocalPdf(System.IO.FileInfo file, int pages)
        {
            Assert.IsTrue(file.Exists);
            Assert.AreNotEqual(0, file.Length);
            using (Pdf pdf = Pdf.CreateReader(file.FullName))
            {
                Assert.AreEqual(pages, pdf.GetPagesAmount());
            }
        }
    }
}