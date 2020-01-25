using GoogleDocumentsUnifier.Logic;

namespace MoscowNvcBot.Web.Models
{
    internal class GooglePdfData
    {
        public enum FileStatus
        {
            None,
            Outdated,
            Ok
        }

        public readonly string Name;
        public readonly string Id;
        public readonly string SourceId;
        public readonly FileStatus Status;

        public static GooglePdfData CreateNone(string sourceId, string name)
        {
            return new GooglePdfData(FileStatus.None, sourceId, name);
        }

        public static GooglePdfData CreateOutdated(string sourceId, string id)
        {
            return new GooglePdfData(FileStatus.Outdated, sourceId, null, id);
        }

        public static GooglePdfData CreateOk() => new GooglePdfData(FileStatus.Ok);

        private GooglePdfData(FileStatus status, string sourceId = null, string name = null, string id = null)
        {
            Name = name;
            Id = id;
            SourceId = sourceId;
            Status = status;
        }
    }
}

