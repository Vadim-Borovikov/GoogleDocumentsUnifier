using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace GoogleDocumentsUnifier.Logic.Legacy
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

        public string GetName(string id) => _provider.GetName(id);

        public void Copy(DocumentRequest request, string resultPath)
        {
            using (Pdf pdfWriter = Pdf.CreateWriter(resultPath))
            {
                Import(request, pdfWriter);
            }
        }

        public void Unify(IEnumerable<DocumentRequest> requests, string resultPath)
        {
            using (Pdf pdfWriter = Pdf.CreateWriter(resultPath))
            {
                foreach (DocumentRequest request in requests)
                {
                    Import(request, pdfWriter);
                }
            }
        }

        private void Import(DocumentRequest request, Pdf pdfWriter)
        {
            bool tempFileNeeded = request.Info.DocumentType != DocumentType.LocalPdf;

            string path;

            if (tempFileNeeded)
            {
                using (var stream = new MemoryStream())
                {
                    SetupStream(stream, request.Info);
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

        private void SetupStream(Stream stream, DocumentInfo info)
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
                    _provider.DownloadFile(info.Id, stream);
                    break;
                case DocumentType.GoogleDocument:
                    _provider.ExportFile(info.Id, PdfMimeType, stream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info.DocumentType));
            }
        }

        private const string PdfMimeType = "application/pdf";
        private readonly GoogleApisDriveProvider _provider;
    }
}
