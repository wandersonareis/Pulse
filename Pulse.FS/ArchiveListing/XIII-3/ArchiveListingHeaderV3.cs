using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingHeaderV3 : IArchiveListingHeader, IStreamingContent
    {
        private const int KeyDataSize = CryptFilelist.CryptHeaderSize;

        // Streaming Data
        public readonly byte[] KeyData = new byte[KeyDataSize];
        public int RawBlockOffset;
        public int RawInfoOffset;
        public int EntriesCount { get; set; }

        /// <summary>
        /// True when the filelist on the disk carries the crypto header.
        /// </summary>
        public bool HasCryptHeader;

        /// <summary>
        /// True when the body of the filelist on the disk is encrypted.
        /// </summary>
        public bool IsEncrypted;

        /// <summary>
        /// The offset of the logical body of the filelist: 32 for the filelists with
        /// the crypto header, 0 for the plain ones.
        /// </summary>
        public int BaseOffset => HasCryptHeader ? KeyDataSize : 0;

        public int BlockOffset => BaseOffset + RawBlockOffset;
        public int InfoOffset => BaseOffset + RawInfoOffset;
        public int BlocksCount => (RawInfoOffset - RawBlockOffset) / 12;

        public bool IsValid(long fileSize)
        {
            if (RawBlockOffset > RawInfoOffset || RawBlockOffset < 0 || RawInfoOffset < 0)
                return false;

            if (EntriesCount < 0)
                return false;

            if (BlocksCount > short.MaxValue)
                return false;

            if (BlockOffset > fileSize || InfoOffset > fileSize)
                return false;

            return true;
        }

        public void ReadFromStream(Stream stream)
        {
            stream.EnsureRead(KeyData, 0, KeyDataSize);

            HasCryptHeader = BitConverter.ToUInt32(KeyData, 20) == CryptFilelist.EncryptionTag;

            if (HasCryptHeader)
            {
                BinaryReader br = new(stream);
                RawBlockOffset = br.ReadInt32();
                RawInfoOffset = br.ReadInt32();
                EntriesCount = br.ReadInt32();
            }
            else
            {
                RawBlockOffset = BitConverter.ToInt32(KeyData, 0);
                RawInfoOffset = BitConverter.ToInt32(KeyData, 4);
                EntriesCount = BitConverter.ToInt32(KeyData, 8);

                // the body of a plain filelist starts right after its own header
                stream.Position = 12;
            }
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new(stream);

            if (HasCryptHeader)
                stream.Write(KeyData, 0, KeyDataSize);

            bw.Write(RawBlockOffset);
            bw.Write(RawInfoOffset);
            bw.Write(EntriesCount);
        }
    }
}
