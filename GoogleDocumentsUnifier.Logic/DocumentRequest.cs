namespace GoogleDocumentsUnifier.Logic
{
    public class DocumentRequest
    {
        internal readonly string Path;
        public uint Amount;

        public DocumentRequest(string path, uint amount)
        {
            Path = path;
            Amount = amount;
        }
    }
}
