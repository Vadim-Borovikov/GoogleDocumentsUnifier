using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace GoogleDocumentsUnifier.Logic
{
    public class Pdf : IDisposable
    {
        public Pdf()
        {
            _document = new PdfDocument();
        }

        public Pdf(string inputPath)
        {
            _document = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);

            int _ = _document.PageCount; // Otherwise Save test failing LOL
        }

        public Pdf(Stream inputStream)
        {
            _document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Import);
        }

        public void Dispose()
        {
            _document.Close();
            _document.Dispose();
        }

        public void AddAllPages(Pdf pdf)
        {
            for (int i = 0; i < pdf.PagesAmount; ++i)
            {
                _document.AddPage(pdf._document.Pages[i]);
            }
        }

        public void Save(string path)
        {
            _document.Save(path);
        }

        private readonly PdfDocument _document;

        public int PagesAmount => _document.PageCount;
    }
}
