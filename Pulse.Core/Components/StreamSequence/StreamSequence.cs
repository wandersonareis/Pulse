using System;
using System.IO;

namespace Pulse.Core
{
    public sealed class StreamSequence : Stream
    {
        private readonly ISequencedStreamFactory _factory;

        public StreamSequence(ISequencedStreamFactory factory)
        {
            _factory = factory;

            Exception ex;
            if (!_factory.TryCreateNextStream(null, out _current, out ex))
                throw ex;
        }

        private Stream _current;

        public bool TryCreateNextStream(string key)
        {
            _current.NullSafeDispose();

            Exception ex;
            return _factory.TryCreateNextStream(key, out _current, out ex);
        }

        public override void Close()
        {
            _current?.Close();
        }

        public override void Flush()
        {
            _current?.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _current.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _current.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _current.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _current.Write(buffer, offset, count);
        }

        public override bool CanRead => _current.CanRead;

        public override bool CanSeek => _current.CanSeek;

        public override bool CanWrite => _current.CanWrite;

        public override long Length => _current.Length;

        public override long Position
        {
            get => _current.Position;
            set => _current.Position = value;
        }
    }
}