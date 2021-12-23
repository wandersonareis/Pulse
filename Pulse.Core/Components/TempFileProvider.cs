using System;
using System.IO;
using System.Threading;

namespace Pulse.Core
{
    public sealed class TempFileProvider : IDisposable
    {
        public readonly string FilePath;
        public readonly string FilePrefix;
        public readonly string Extension;
        private long _counter;

        public long Size { get; set; }

        public TempFileProvider()
            : this(string.Empty, string.Empty)
        {
        }

        public TempFileProvider(string filePrefix, string extension)
        {
            FilePrefix = filePrefix;
            Extension = extension;
            FilePath = Path.GetTempPath() + filePrefix + Guid.NewGuid() + extension;
        }

        public Stream Create()
        {
            return Acquire(File.Create(FilePath));
        }

        public Stream OpenRead()
        {
            return Acquire(File.OpenRead(FilePath));
        }

        private Stream Acquire(Stream stream)
        {
            try
            {
                Interlocked.Increment(ref _counter);
                DisposableStream result = new DisposableStream(stream);
                result.AfterDispose.Add(new DisposableAction(Dispose));
                return result;
            }
            catch
            {
                stream.SafeDispose();
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                if (Interlocked.Decrement(ref _counter) >= 0)
                    return;

                if (File.Exists(FilePath))
                    File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}