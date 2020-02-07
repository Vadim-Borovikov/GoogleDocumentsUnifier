namespace MoscowNvcBot.Web.Models
{
    internal class PdfData
    {
        public enum FileStatus
        {
            None,
            Outdated,
            Ok
        }

        public readonly string Name;
        public readonly string IdOrPath;
        public readonly string SourceId;
        public readonly FileStatus Status;

        public static PdfData CreateNone(string sourceId, string name)
        {
            return new PdfData(FileStatus.None, sourceId, name);
        }

        public static PdfData CreateOutdated(string sourceId, string idOrPath)
        {
            return new PdfData(FileStatus.Outdated, sourceId, null, idOrPath);
        }

        public static PdfData CreateOk() => new PdfData(FileStatus.Ok);

        private PdfData(FileStatus status, string sourceId = null, string name = null, string id = null)
        {
            Name = name;
            IdOrPath = id;
            SourceId = sourceId;
            Status = status;
        }
    }
}

