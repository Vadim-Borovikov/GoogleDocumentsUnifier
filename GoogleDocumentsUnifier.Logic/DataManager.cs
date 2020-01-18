using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GoogleDocumentsUnifier.Logic
{
    public class DataManager : IDisposable
    {
        public DataManager(string projectJson)
        {
            _provider = new GoogleApisDriveProvider(projectJson);
        }

        public void Dispose()
        {
            _provider.Dispose();
        }

        public async Task<string> GetNameAsync(string id) => await _provider.GetNameAsync(id);

        public async Task CopyAsync(DocumentRequest request, string resultPath)
        {
            using (Pdf pdfWriter = Pdf.CreateWriter(resultPath))
            {
                await ImportAsync(request, pdfWriter);
            }
        }

        public async Task UnifyAsync(IEnumerable<DocumentRequest> requests, string resultPath)
        {
            using (Pdf pdfWriter = Pdf.CreateWriter(resultPath))
            {
                foreach (DocumentRequest request in requests)
                {
                    await ImportAsync(request, pdfWriter);
                }
            }
        }

        private async Task ImportAsync(DocumentRequest request, Pdf pdfWriter)
        {
            bool tempFileNeeded = request.Info.DocumentType != DocumentType.LocalPdf;

            string path;

            if (tempFileNeeded)
            {
                using (var stream = new MemoryStream())
                {
                    await SetupStreamAsync(stream, request.Info);
                    path = Path.GetTempFileName();
                    using (var fileStream = new FileStream(path, FileMode.Open))
                    {
                        stream.WriteTo(fileStream);
                    }
                }
            }
            else
            {
                path = request.Info.Id;
            }

            using (Pdf pdfReader = Pdf.CreateReader(path))
            {
                for (uint i = 0; i < request.Amount; ++i)
                {
                    pdfWriter.AddAllPages(pdfReader);
                }
            }

            if (tempFileNeeded)
            {
                File.Delete(path);
            }
        }

        private async Task SetupStreamAsync(Stream stream, DocumentInfo info)
        {
            switch (info.DocumentType)
            {
                case DocumentType.LocalPdf:
                    throw new ArgumentOutOfRangeException(nameof(info.DocumentType), "Local pdf doesn't need stream!");
                case DocumentType.WebPdf:
                    using (var client = new WebClient())
                    {
                        byte[] data = client.DownloadData(info.Id);
                        stream.Write(data, 0, data.Length);
                    }
                    break;
                case DocumentType.GooglePdf:
                    await _provider.DownloadFileAsync(info.Id, stream);
                    break;
                case DocumentType.GoogleDocument:
                    await _provider.ExportFileAsync(info.Id, PdfMimeType, stream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info.DocumentType));
            }
        }

        private const string PdfMimeType = "application/pdf";
        private readonly GoogleApisDriveProvider _provider;
    }
}
