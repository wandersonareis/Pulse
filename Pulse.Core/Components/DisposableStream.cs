﻿using System.IO;

namespace Pulse.Core
{
    public sealed class DisposableStream : Stream
    {
        private readonly Stream _stream;

        public readonly DisposableStack BeforeDispose = new(0);
        public readonly DisposableStack AfterDispose = new(0);

        public DisposableStream(Stream stream)
        {
            _stream = stream;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            BeforeDispose.SafeDispose();
            _stream.Dispose();
            AfterDispose.SafeDispose();
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }
    }
}