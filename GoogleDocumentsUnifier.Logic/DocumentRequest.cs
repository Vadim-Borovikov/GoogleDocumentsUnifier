﻿namespace GoogleDocumentsUnifier.Logic
{
    public class DocumentRequest
    {
        public readonly DocumentInfo Info;
        internal readonly uint Amount;

        public DocumentRequest(DocumentInfo info, uint amount = 1)
        {
            Info = info;
            Amount = amount;
        }
    }
}
