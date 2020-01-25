using System;
using System.IO;

namespace GoogleDocumentsUnifier.Logic.Tests
{
    internal class TempFile : IDisposable
    {
        public readonly System.IO.FileInfo File;

        public TempFile()
        {
            string path = Path.GetTempFileName();
            File = new System.IO.FileInfo(path);
        }

        public void Dispose()
        {
            if (File.Exists)
            {
                File.Delete();
            }
        }
    }
}
