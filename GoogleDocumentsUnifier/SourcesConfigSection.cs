using System;
using System.Configuration;
using GoogleDocumentsUnifier.Logic;
// ReSharper disable ClassNeverInstantiated.Global

namespace GoogleDocumentsUnifier
{
    internal class SourcesConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("Sources")]
        public SourcesCollection SourcesItems => (SourcesCollection)base["Sources"];
    }

    [ConfigurationCollection(typeof(SourceElement))]
    public class SourcesCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement() => new SourceElement();

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SourceElement)element).Id;
        }
    }

    public class SourceElement : ConfigurationElement
    {
        [ConfigurationProperty("id", DefaultValue = "", IsKey = true, IsRequired = true)]
        internal string Id => (string)base["id"];

        [ConfigurationProperty("type", DefaultValue = "", IsKey = false, IsRequired = true)]
        private string TypeId => (string)base["type"];

        internal DocumentType Type
        {
            get
            {
                switch (TypeId)
                {
                    case "pdfPath":
                        return DocumentType.LocalPdf;
                    case "pdfUrl":
                        return DocumentType.WebPdf;
                    case "pdfId":
                        return DocumentType.GooglePdf;
                    case "docId":
                        return DocumentType.GoogleDocument;
                    default:
                        throw new ArgumentOutOfRangeException(TypeId);
                }
            }
        }
    }
}
