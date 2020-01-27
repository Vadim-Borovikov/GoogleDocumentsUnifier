using System;
using System.IO;
using System.Threading.Tasks;

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

        internal static TempFile CreateFor<T>(Action<T, string> action, T parameter)
        {
            var result = new TempFile();
            action.Invoke(parameter, result.Path);
            return result;
        }

        internal static async Task<TempFile> CreateForAsync<T>(Func<T, string, Task> func, T parameter)
        {
            var result = new TempFile();
            await func.Invoke(parameter, result.Path);
            return result;
        }
    }
}
