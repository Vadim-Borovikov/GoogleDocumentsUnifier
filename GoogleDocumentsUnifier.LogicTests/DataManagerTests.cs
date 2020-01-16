using System.IO;
using GoogleDocumentsUnifier.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GoogleDocumentsUnifier.LogicTests
{
    [TestClass]
    public class DataManagerTests
    {
        [TestMethod]
        public void UnifyTest()
        {
            string projectJson = File.ReadAllText(ProjectJsonPath);
            using (var dataManager = new DataManager(projectJson))
            {
                var pdf = new DocumentInfo(InputPdfPath, DocumentType.LocalPdf);
                var requests = new[]
                {
                    new DocumentRequest(pdf)
                };
                dataManager.Unify(requests, OutputPdfPath);
            }
        }

        private const string ProjectJsonPath = "Keys/project.json";

        private const string InputPdfPath = "Test/pdf.pdf";
        private const string OutputPdfPath = "Test/result.pdf";
    }
}