﻿namespace GoogleDocumentsUnifier.Logic
{
    public class DocumentInfo
    {
        internal readonly string Id;
        internal readonly DocumentType DocumentType;

        public DocumentInfo(string id, DocumentType documentType)
        {
            Id = id;
            DocumentType = documentType;
        }
    }
}
