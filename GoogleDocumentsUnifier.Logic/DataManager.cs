using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleDocumentsUnifier.Logic
{
    public class DataManager : IDisposable
    {
        public DataManager(string projectJson) { _provider = new GoogleApisDriveProvider(projectJson); }

        public void Dispose() { _provider.Dispose(); }

        public async Task<FileInfo> GetFileInfoAsync(string id) => await _provider.GetFileInfoAsync(id);

        public async Task<FileInfo> FindFileInFolderAsync(string parent, string name)
        {
            IEnumerable<FileInfo> files = await _provider.FindFilesInFolderAsync(parent, name);
            return files.FirstOrDefault();
        }

        public async Task<IEnumerable<FileInfo>> GetFilesInFolderAsync(string parent)
        {
            return await _provider.GetFilesInFolder(parent);
        }

        public async Task<TempFile> DownloadAsync(DocumentInfo info)
        {
            return await TempFile.CreateForAsync(DownloadAsync, info);
        }

        public static TempFile Unify(IEnumerable<DocumentRequest> requests) => TempFile.CreateFor(Unify, requests);

        public async Task CreateAsync(string name, string parentId, string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                await _provider.CreateAsync(name, parentId, stream, PdfMimeType);
            }
        }

        public async Task UpdateAsync(string fileId, string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                await _provider.UpdateAsync(fileId, stream, PdfMimeType);
            }
        }

        private async Task DownloadAsync(DocumentInfo info, string resultPath)
        {
            using (Pdf pdfWriter = Pdf.CreateWriter(resultPath))
            {
                using (var temp = new TempFile())
                {
                    using (var stream = new MemoryStream())
                    {
                        await SetupStreamAsync(stream, info);
                        using (var fileStream = new FileStream(temp.Path, FileMode.Open))
                        {
                            stream.WriteTo(fileStream);
                        }
                    }

                    using (Pdf pdfReader = Pdf.CreateReader(temp.Path))
                    {
                        pdfWriter.AddAllPages(pdfReader);
                    }
                }
            }
        }

        private async Task SetupStreamAsync(Stream stream, DocumentInfo info)
        {
            switch (info.DocumentType)
            {
                case DocumentType.Document:
                    await _provider.ExportFileAsync(info.Id, PdfMimeType, stream);
                    break;
                case DocumentType.Pdf:
                    await _provider.DownloadFileAsync(info.Id, stream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info.DocumentType));
            }
        }

        private static void Unify(IEnumerable<DocumentRequest> requests, string resultPath)
        {
            using (Pdf pdfWriter = Pdf.CreateWriter(resultPath))
            {
                foreach (DocumentRequest request in requests)
                {
                    Add(request, pdfWriter);
                }
            }
        }

        private static void Add(DocumentRequest source, Pdf targetWriter)
        {
            using (Pdf pdfReader = Pdf.CreateReader(source.Path))
            {
                for (uint i = 0; i < source.Amount; ++i)
                {
                    targetWriter.AddAllPages(pdfReader);
                }
            }
        }

        private const string PdfMimeType = "application/pdf";

        private readonly GoogleApisDriveProvider _provider;
    }
}
