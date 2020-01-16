using System;
using System.IO;
using iText.Kernel.Pdf;

namespace GoogleDocumentsUnifier.Logic
{
    public class Pdf : IDisposable
    {
        private PdfDocument _document;

        public static Pdf CreateReader(string inputPath)
        {
            var pdf = new Pdf();

            var reader = new PdfReader(inputPath);

            pdf._document = new PdfDocument(reader);

            pdf._document.SetCloseReader(true);

            return pdf;
        }

        public static Pdf CreateWriter(string outputPath)
        {
            var pdf = new Pdf();

            var writer = new PdfWriter(outputPath);

            pdf._document = new PdfDocument(writer);

            pdf._document.SetCloseWriter(true);

            return pdf;
        }

        private Pdf() { }

        public void Dispose()
        {
            _document.Close();
        }

        public void AddAllPages(Pdf pdf)
        {
            pdf._document.CopyPagesTo(1, pdf.GetPagesAmount(), _document);
        }

        public int GetPagesAmount() => _document.GetNumberOfPages();
    }
}
