using System;
using System.Collections.Generic;
using System.IO;
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

        public void Unify(IEnumerable<DocumentRequest> requests, string resultPath)
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
                            ImportEvens(pdf, result, request.Amount);
                        }
                    }
                }

                result.Save(resultPath);
            }
        }

        private void SetupStream(Stream stream, DocumentInfo info)
        {
            if (info.IsPdfAlready)
            {
                _provider.DownloadFile(info.Id, stream);
            }
            else
            {
                _provider.ExportFile(info.Id, PdfMimeType, stream);
            }
        }

        private static void ImportEvens(Pdf source, Pdf target, uint amount)
        {
            MakePdfPagesCountEven(source);

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
