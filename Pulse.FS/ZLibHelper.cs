using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Pulse.Core;

namespace Pulse.FS
{
    public static class ZLibHelper
    {
        public static byte[] Uncompress(Stream input, int uncompressedSize)
        {
            byte[] buff = new byte[uncompressedSize];
            using (ZLibStream reader = new(input, CompressionMode.Decompress, true))
                reader.EnsureRead(buff, 0, uncompressedSize);
            return buff;
        }

        public static void UncompressAndDisposeStreams(Stream input, Stream output, int uncompressedSize, CancellationToken cancelationToken, Action<long> uncompressed = null)
        {
            try
            {
                if (cancelationToken.IsCancellationRequested)
                    return;

                byte[] buff = new byte[Math.Min(32 * 1024, uncompressedSize)];
                Uncompress(input, output, uncompressedSize, buff, cancelationToken, uncompressed);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                output.SafeDispose();
                input.SafeDispose();
            }
        }

        public static void Uncompress(Stream input, Stream output, int uncompressedSize, byte[] buff, CancellationToken cancelationToken, Action<long> uncompressed = null)
        {
            Exceptions.CheckArgumentNull(input, "input");
            Exceptions.CheckArgumentNull(output, "output");
            Exceptions.CheckArgumentOutOfRangeException(uncompressedSize, "uncompressedSize", 0, int.MaxValue);

            using ZLibStream reader = new(input, CompressionMode.Decompress, true);

            int readed;
            while (uncompressedSize > 0 && (readed = reader.Read(buff, 0, Math.Min(buff.Length, uncompressedSize))) > 0)
            {
                if (cancelationToken.IsCancellationRequested)
                    return;

                uncompressedSize -= readed;
                output.Write(buff, 0, readed);
                uncompressed.NullSafeInvoke(readed);
            }

            if (uncompressedSize != 0)
                throw new("??????????? ????? ??????.");
        }

        public static int Compress(Stream input, Stream output, int uncompressedSize, Action<long> compressed = null)
        {
            Exceptions.CheckArgumentNull(input, "input");
            Exceptions.CheckArgumentNull(output, "output");
            Exceptions.CheckArgumentOutOfRangeException(uncompressedSize, "uncompressedSize", 0, int.MaxValue);

            byte[] buff = new byte[Math.Min(32 * 1024, uncompressedSize)];

            long position = output.Position;

            using (ZLibStream writer = new(output, CompressionLevel.Optimal, true))
            {
                int readed;
                while (uncompressedSize > 0 && (readed = input.Read(buff, 0, Math.Min(buff.Length, uncompressedSize))) > 0)
                {
                    uncompressedSize -= readed;
                    writer.Write(buff, 0, readed);
                    compressed.NullSafeInvoke(readed);
                }
            }

            if (uncompressedSize != 0)
                throw new("??????????? ????? ??????.");

            return (int)(output.Position - position);
        }
    }
}
