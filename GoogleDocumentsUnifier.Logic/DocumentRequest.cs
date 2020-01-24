namespace GoogleDocumentsUnifier.Logic
{
    public class DocumentRequest
    {
        public uint Amount;

        internal readonly DocumentInfo Info;

        public DocumentRequest(DocumentInfo info, uint amount)
        {
            Info = info;
            Amount = amount;
        }
    }
}
