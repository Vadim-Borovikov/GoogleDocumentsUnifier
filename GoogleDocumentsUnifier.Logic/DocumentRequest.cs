namespace GoogleDocumentsUnifier.Logic
{
    public class DocumentRequest
    {
        internal readonly DocumentInfo Info;
        public uint Amount;

        public DocumentRequest(DocumentInfo info, uint amount)
        {
            Info = info;
            Amount = amount;
        }
    }
}
