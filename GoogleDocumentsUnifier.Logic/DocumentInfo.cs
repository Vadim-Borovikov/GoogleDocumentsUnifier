namespace GoogleDocumentsUnifier.Logic
{
    public class DocumentInfo
    {
        public readonly string Id;
        public readonly bool IsPdfAlready;

        public DocumentInfo(string id, bool isPdfAlready)
        {
            Id = id;
            IsPdfAlready = isPdfAlready;
        }
    }
}
