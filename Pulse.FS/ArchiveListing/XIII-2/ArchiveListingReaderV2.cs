using System;
using System.Globalization;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingReaderV2 : IDisposable
    {
        public static ArchiveListing Read(ArchiveAccessor accessor, Action<long> progressIncrement, Action<long> progressTotalChanged)
        {
            using (ArchiveListingReaderV2 reader = new(accessor, progressIncrement, progressTotalChanged))
            {
                ArchiveListing result = reader.Read();
                return result;
            }
        }

        private readonly ArchiveAccessor _accessor;
        private readonly Action<long> _progressIncrement;
        private readonly Action<long> _progressTotalChanged;

        private ArchiveListingReaderV2(ArchiveAccessor accessor, Action<long> progressIncrement, Action<long> progressTotalChanged)
        {
            _accessor = accessor;
            _progressIncrement = progressIncrement;
            _progressTotalChanged = progressTotalChanged;
        }

        public void Dispose()
        {
        }

        public ArchiveListing Read()
        {
            ArchiveListingHeaderV2 header;
            using (Stream input = Decrypt(out header))
            {

                _progressTotalChanged.NullSafeInvoke(header.EntriesCount);

                short blockNumber = -1;
                bool? flag = null;

                ArchiveListingEntryInfoV2[] entries = new ArchiveListingEntryInfoV2[header.EntriesCount];
                for (int i = 0; i < entries.Length; i++)
                {
                    entries[i] = input.ReadContent<ArchiveListingEntryInfoV2>();
                    if (entries[i].Flag != flag)
                    {
                        flag = entries[i].Flag;
                        blockNumber++;
                    }
                    entries[i].BlockNumber = blockNumber;
                }

                ArchiveListingCompressedData data = new(header);
                data.ReadFromStream(input);

                ArchiveListing result = new(_accessor, header);
                ParseEntries(entries, data, result);
                return result;
            }
        }

        private Stream Decrypt(out ArchiveListingHeaderV2 header)
        {
            byte[] source = ExtractListingData();

            byte[] plain = source;
            bool isEncrypted = false;

            if (CryptFilelist.HasCryptHeader(source) && !CryptFilelist.IsPlainText(source))
            {
                plain = CryptFilelist.Decrypt(source);
                isEncrypted = true;
            }

            MemoryStream result = new(plain, false);
            try
            {
                header = result.ReadContent<ArchiveListingHeaderV2>();
                header.IsEncrypted = isEncrypted;

                if (!header.IsValid(result.Length))
                    throw new InvalidDataException("The header of the filelist is not valid.");

                return result;
            }
            catch
            {
                result.SafeDispose();
                throw;
            }
        }

        private byte[] ExtractListingData()
        {
            using (Stream input = _accessor.ExtractListing())
            using (MemoryStream buffer = new((int)input.Length))
            {
                input.CopyTo(buffer);
                return buffer.ToArray();
            }
        }

        private void ParseEntries(ArchiveListingEntryInfoV2[] entries, ArchiveListingCompressedData data, ArchiveListing result)
        {
            byte[] buff = new byte[0];

            for (int currentBlock = -1, i = 0; i < entries.Length; i++)
            {
                ArchiveListingEntryInfoV2 entryInfoV2 = entries[i];
                if (entryInfoV2.BlockNumber != currentBlock)
                {
                    currentBlock = entryInfoV2.BlockNumber;
                    buff = data.AcquireData(currentBlock);
                }

                string name;
                long sector, uncompressedSize, compressedSize;
                ParseInfo(entryInfoV2, buff, out sector, out uncompressedSize, out compressedSize, out name);

                ArchiveEntry entry = new(name, sector, compressedSize, uncompressedSize)
                {
                    UnknownNumber = entryInfoV2.UnknownNumber,
                    UnknownValue = entryInfoV2.UnknownValue,
                    UnknownData = entryInfoV2.UnknownData
                };

                result.Add(entry);
                _progressIncrement.NullSafeInvoke(1);
            }
        }

        private void ParseInfo(ArchiveListingEntryInfoV2 entryInfo, byte[] uncompressedData, out long sector, out long uncompressedSize, out long compressedSize, out string name)
        {
            string[] info;
            unsafe
            {
                fixed (byte* ptr = &uncompressedData[entryInfo.Offset])
                {
                    string str = new((sbyte*)ptr);
                    info = str.Split(':');
                }
            }

            if (info.Length < 4)
            {
                name = string.Join(":", info);
                sector = -1;
                uncompressedSize = -1;
                compressedSize = -1;
            }
            else
            {
                sector = long.Parse(info[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                uncompressedSize = long.Parse(info[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                compressedSize = long.Parse(info[2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                name = info[3];
            }
        }
    }
}
