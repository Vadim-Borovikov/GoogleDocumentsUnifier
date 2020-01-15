﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace GoogleDocumentsUnifier.Logic
{
    public class DataManager : IDisposable
    {
        public DataManager(string projectJson)
        {
            using (var stream = new FileStream(projectJson, FileMode.Open, FileAccess.Read))
            {
                _provider = new GoogleApisDriveProvider(stream);
            }
        }

        public void Dispose()
        {
            _provider.Dispose();
        }

        public string GetName(string id) => _provider.GetName(id);

        public void Copy(DocumentRequest request, string resultPath, bool makeEvens)
        {
            using (var result = new Pdf())
            {
                Import(request, result, makeEvens);

                result.Save(resultPath);
            }
        }

        public void Unify(IEnumerable<DocumentRequest> requests, string resultPath, bool makeEvens)
        {
            using (var result = new Pdf())
            {
                foreach (DocumentRequest request in requests)
                {
                    Import(request, result, makeEvens);
                }

                result.Save(resultPath);
            }
        }

        private void Import(DocumentRequest request, Pdf result, bool makeEvens)
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
