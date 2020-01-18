﻿using System.IO;
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
                        IDownloadProgress progress = await provider.DownloadFileAsync(PdfId, stream);
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
                        IDownloadProgress progress = await provider.ExportFileAsync(DocId, "application/pdf", stream);
                        Assert.AreEqual(DownloadStatus.Completed, progress.Status);
                    }
                }
                CheckFile(temp.File);
            }
        }

        private static GoogleApisDriveProvider CreateProvider()
        {
            string projectJson = File.ReadAllText(ProjectJsonPath);
            return new GoogleApisDriveProvider(projectJson);
        }

        private static void CheckFile(FileInfo file)
        {
            Assert.IsTrue(file.Exists);
            Assert.AreNotEqual(0, file.Length);
        }

        private const string ProjectJsonPath = "Keys/project.json";

        private const string PdfId = "17hnk4p5kIS8U4vK5JB18B59WMy-dEVvk";
        private const string DocId = "1WxXjtQu03JfLR5dhECNWcYX8EyoHUJePDrUocQnsB8g";
    }
}