using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingCompressedData : IStreamingContent
    {
        private readonly IArchiveListingHeader _header;

        private readonly byte[][] _uncompressedBlocks;
        private volatile Exception _exception;
        private volatile bool _completed;

        public ArchiveListingCompressedData(IArchiveListingHeader header)
        {
            _header = header;
            _uncompressedBlocks = new byte[_header.BlocksCount][];
        }

        public void ReadFromStream(Stream stream)
        {
            stream.Position = _header.BlockOffset;
            ArchiveListingBlockInfo[] blocks = stream.ReadStructs<ArchiveListingBlockInfo>(_header.BlocksCount);

            // The compressed data is copied to the memory, because the decompression
            // is performed in the background thread and the source stream may be
            // disposed before the thread is completed.
            int dataSize = 0;
            for (int i = 0; i < blocks.Length; i++)
                dataSize = Math.Max(dataSize, blocks[i].Offset + blocks[i].CompressedSize);

            byte[] compressedData = new byte[dataSize];
            if (dataSize > 0)
            {
                stream.Position = _header.InfoOffset;
                stream.EnsureRead(compressedData, 0, dataSize);
            }

            ThreadHelper.StartBackground("ArchiveListingCompressedData.ReadFromStream", () => UncompressBlocks(blocks, compressedData));
        }

        public byte[] AcquireData(int blockNumber)
        {
            while (true)
            {
                byte[] result = _uncompressedBlocks[blockNumber];
                if (result != null)
                    return result;

                if (_exception != null)
                    ExceptionDispatchInfo.Capture(_exception).Throw();

                if (_completed)
                    throw new InvalidDataException($"The block {blockNumber} of the listing is missing.");

                Thread.Sleep(100);
            }
        }

        private void UncompressBlocks(ArchiveListingBlockInfo[] blocks, byte[] compressedData)
        {
            try
            {
                for (int i = 0; i < blocks.Length; i++)
                {
                    ArchiveListingBlockInfo block = blocks[i];
                    int uncompressedSize = block.UncompressedSize;

                    using (MemoryStream ms = new(compressedData, block.Offset, block.CompressedSize, false))
                        _uncompressedBlocks[i] = ZLibHelper.Uncompress(ms, uncompressedSize);
                }
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
            finally
            {
                _completed = true;
            }
        }

        public void WriteToStream(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
