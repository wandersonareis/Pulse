using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingWriterV2
    {
        public static void Write(ArchiveListing listing)
        {
            ArchiveListingWriterV2 writer = new(listing);
            writer.Write();
        }

        private readonly ArchiveListing _listing;
        private readonly ArchiveAccessor _accessor;

        private ArchiveListingWriterV2(ArchiveListing listing)
        {
            _listing = listing;
            _accessor = _listing.Accessor;
        }

        public void Write()
        {
            using (MemoryStream headerBuff = new(32768))
            using (MemoryStream textBuff = new(32768))
            {
                ArchiveListingBlockInfo[] blocksInfo;
                ArchiveListingEntryInfoV2[] entriesInfoV2;
                ArchiveListingTextWriterV2 textWriter = new(textBuff);
                textWriter.Write(_listing, out blocksInfo, out entriesInfoV2);

                for (int i = 0; i < entriesInfoV2.Length; i++)
                {
                    ArchiveListingEntryInfoV2 info = entriesInfoV2[i];
                    ArchiveEntry entry = _listing[i];
                    info.UnknownNumber = entry.UnknownNumber;
                    info.UnknownValue = entry.UnknownValue;
                    info.UnknownData = entry.UnknownData;
                }

                byte[] buff = new byte[8192];
                int blocksSize = (int)textBuff.Position;
                textBuff.Position = 0;

                ArchiveListingHeaderV2 header = (ArchiveListingHeaderV2)_listing.Header;
                header.EntriesCount = entriesInfoV2.Length;
                header.RawBlockOffset = entriesInfoV2.Length * 8 + 12;
                header.RawInfoOffset = header.RawBlockOffset + blocksInfo.Length * 12;

                headerBuff.WriteContent(header);
                foreach (ArchiveListingEntryInfoV2 entry in entriesInfoV2)
                    headerBuff.WriteContent(entry);
                foreach (ArchiveListingBlockInfo block in blocksInfo)
                    headerBuff.WriteStruct(block);

                int hederSize = (int)headerBuff.Length;
                headerBuff.Position = 0;

                if (header.HasCryptHeader)
                {
                    RecreateEncryptedListing(headerBuff, hederSize, textBuff, blocksSize);
                }
                else
                {
                    using (Stream output = _accessor.RecreateListing(hederSize + blocksSize))
                    {
                        headerBuff.CopyToStream(output, hederSize, buff);
                        textBuff.CopyToStream(output, blocksSize, buff);
                    }
                }
            }
        }

        private void RecreateEncryptedListing(MemoryStream headerBuff, int hederSize, MemoryStream textBuff, int blocksSize)
        {
            byte[] plain = new byte[hederSize + blocksSize];

            headerBuff.Position = 0;
            headerBuff.EnsureRead(plain, 0, hederSize);

            textBuff.Position = 0;
            textBuff.EnsureRead(plain, hederSize, blocksSize);

            byte[] encrypted = CryptFilelist.Encrypt(plain);

            using (Stream output = _accessor.RecreateListing(encrypted.Length))
                output.Write(encrypted, 0, encrypted.Length);
        }
    }
}
