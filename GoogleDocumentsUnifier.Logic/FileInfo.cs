using System;

namespace GoogleDocumentsUnifier.Logic
{
    public class FileInfo
    {
        public readonly string Id;
        public readonly string Name;
        public readonly DateTime? ModifiedTime;

        internal FileInfo(string id, string name, DateTime? modifiedTime)
        {
            Id = id;
            Name = name;
            ModifiedTime = modifiedTime;
        }
    }
}
