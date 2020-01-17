namespace GoogleDocumentsUnifier.Logic.Legacy
{
    public class DocumentInfo
    {
        public readonly string Id;
        public readonly DocumentType DocumentType;

        public DocumentInfo(string id, DocumentType documentType)
        {
            Id = id;
            DocumentType = documentType;
        }
    }
}
