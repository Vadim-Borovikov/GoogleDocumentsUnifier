﻿using System;
using System.IO;

namespace GoogleDocumentsUnifier.LogicTests
{
    internal class TempFile : IDisposable
    {
        public readonly FileInfo File;

        public TempFile()
        {
            string path = Path.GetTempFileName();
            File = new FileInfo(path);
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
