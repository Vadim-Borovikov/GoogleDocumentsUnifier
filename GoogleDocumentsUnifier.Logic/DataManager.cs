using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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

        public string GetName(string id) => _provider.GetName(id);

        public void Copy(DocumentRequest request, string resultPath)
        {
            using (var result = new Pdf())
            {
                Import(request, result);

                result.Save(resultPath);
            }
        }

        public void Unify(IEnumerable<DocumentRequest> requests, string resultPath)
        {
            using (var result = new Pdf())
            {
                foreach (DocumentRequest request in requests)
                {
                    Import(request, result);
                }

                result.Save(resultPath);
            }
        }

        private void Import(DocumentRequest request, Pdf result)
        {
            using (var stream = new MemoryStream())
            {
                SetupStream(stream, request.Info);
                using (var pdf = new Pdf(stream))
                {
                    for (uint i = 0; i < request.Amount; ++i)
                    {
                        result.AddAllPages(pdf);
                    }
                }
            }
        }

        private void SetupStream(Stream stream, DocumentInfo info)
        {
            switch (info.DocumentType)
            {
                case DocumentType.LocalPdf:
                    using (var file = new FileStream(info.Id, FileMode.Open, FileAccess.Read))
                    {
                        file.CopyTo(stream);
                    }
                    break;
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
