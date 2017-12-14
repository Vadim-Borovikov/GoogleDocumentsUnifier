using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace GoogleDocumentsUnifier.Logic
{
    public class DataManager : IDisposable
    {
        public DataManager(string clientSecretJson)
        {
            using (var stream = new FileStream(clientSecretJson, FileMode.Open, FileAccess.Read))
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string credentialPath = Path.Combine(folderPath, ".credentials/drive-dotnet-quickstart.json");

                _provider = new GoogleApisDriveProvider(stream, credentialPath, "user", CancellationToken.None);
            }

        }

        public void Dispose()
        {
            _provider.Dispose();
        }

        public string GetName(string id) => _provider.GetName(id);

        public void Unify(IEnumerable<DocumentRequest> requests, string resultPath, bool makeEvens)
        {
            using (var result = new Pdf())
            {
                foreach (DocumentRequest request in requests)
                {
                    using (var stream = new MemoryStream())
                    {
                        SetupStream(stream, request.Info);
                        using (var pdf = new Pdf(stream))
                        {
                            Import(pdf, result, request.Amount, makeEvens);
                        }
                    }
                }

                result.Save(resultPath);
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

        private static void Import(Pdf source, Pdf target, uint amount, bool makeEvens)
        {
            if (makeEvens)
            {
                MakePdfPagesCountEven(source);
            }

            for (uint i = 0; i < amount; ++i)
            {
                target.AddAllPages(source);
            }
        }

        private static void MakePdfPagesCountEven(Pdf pdf)
        {
            if ((pdf.PagesAmount % 2) != 0)
            {
                pdf.AddEmptyPage();
            }
        }

        private const string PdfMimeType = "application/pdf";
        private readonly GoogleApisDriveProvider _provider;
    }
}
