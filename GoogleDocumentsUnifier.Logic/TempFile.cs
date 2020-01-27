using System;
using System.IO;

namespace GoogleDocumentsUnifier.Logic
{
    public class TempFile : IDisposable
    {
        public readonly string Path;

        public TempFile()
        {
            Path = System.IO.Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
