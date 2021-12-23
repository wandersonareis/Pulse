using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingWriterV3
    {
        public static void Write(ArchiveListing listing)
        {
            ArchiveListingWriterV3 writer = new ArchiveListingWriterV3(listing);
            writer.Write();
        }

        private readonly ArchiveListing _listing;
        private readonly ArchiveAccessor _accessor;

        private ArchiveListingWriterV3(ArchiveListing listing)
        {
            _listing = listing;
            _accessor = _listing.Accessor;
        }

        public void Write()
        {
            using (MemoryStream headerBuff = new MemoryStream(32768))
            using (MemoryStream textBuff = new MemoryStream(32768))
            {
                ArchiveListingTextWriterV3 textWriter = new ArchiveListingTextWriterV3(textBuff);
                textWriter.Write(_listing, out ArchiveListingBlockInfo[] blocksInfo, out ArchiveListingEntryInfoV3[] entriesInfoV3);

                for (int i = 0; i < entriesInfoV3.Length; i++)
                {
                    ArchiveListingEntryInfoV3 info = entriesInfoV3[i];
                    ArchiveEntry entry = _listing[i];
                    info.UnknownNumber = entry.UnknownNumber;
                    info.UnknownValue = entry.UnknownValue;
                    info.UnknownData = entry.UnknownData;
                }

                byte[] buff = new byte[8192];
                int blocksSize = (int)textBuff.Position;
                textBuff.Position = 0;

                ArchiveListingHeaderV3 header = (ArchiveListingHeaderV3)_listing.Header;
                header.EntriesCount = entriesInfoV3.Length;
                header.RawBlockOffset = entriesInfoV3.Length * 8 + 12;
                header.RawInfoOffset = header.RawBlockOffset + blocksInfo.Length * 12;

                headerBuff.WriteContent(header);
                for (int i = 0; i < entriesInfoV3.Length; i++)
                {
                    ArchiveListingEntryInfoV3 entry = entriesInfoV3[i];
                    headerBuff.WriteContent(entry);
                }

                for (int i = 0; i < blocksInfo.Length; i++)
                {
                    ArchiveListingBlockInfo block = blocksInfo[i];
                    headerBuff.WriteStruct(block);
                }

                int hederSize = (int)headerBuff.Length;
                headerBuff.Position = 0;

                if (header.IsEncrypted)
                {
                    RecreateEncryptedListing(headerBuff, hederSize, textBuff, blocksSize, buff);
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

        private void RecreateEncryptedListing(MemoryStream headerBuff, int hederSize, MemoryStream textBuff, int blocksSize, byte[] buff)
        {
            using (TempFileProvider tmpProvider = new TempFileProvider("filelist", ".win32.bin"))
            {
                using (Stream output = tmpProvider.Create())
                {
                    headerBuff.CopyToStream(output, hederSize, buff);
                    textBuff.CopyToStream(output, blocksSize, buff);
                }

                Stream stream = tmpProvider.OpenRead();
                tmpProvider.Size = hederSize + blocksSize + 11;

                long size = new FileInfo(tmpProvider.FilePath).Length;
                string num = (hederSize + blocksSize + 11).ToString("X8");
                string hex = tmpProvider.Size.ToString("x8");
                //WriteChecksum(tmpProvider);

                Process encrypter = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"Resources\Executable\ffxiiicrypt.exe",
                        Arguments = $"-e \"{tmpProvider.FilePath}\" filelist",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                encrypter.Start();
                Task<string> erroMessage = encrypter.StandardError.ReadToEndAsync();
                Task<string> outputMessage = encrypter.StandardOutput.ReadToEndAsync();
                encrypter.WaitForExit();
                if (encrypter.ExitCode != -2)
                {
                    StringBuilder sb = new StringBuilder("Decryption error! Code: ");
                    sb.AppendLine(encrypter.ExitCode.ToString());
                    sb.AppendLine("Error: ");
                    sb.AppendLine(erroMessage.Result);
                    sb.AppendLine("Output: ");
                    sb.AppendLine(outputMessage.Result);

                    throw new InvalidDataException(sb.ToString());
                }

                using (Stream input = tmpProvider.OpenRead())
                using (Stream output = _accessor.RecreateListing((int)input.Length))
                    input.CopyToStream(output, (int)input.Length, buff);
            }
        }
        public static void WriteChecksum(TempFileProvider tmpProvider)
        {
            Process encrypter = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = @"Resources\Executable\ffxiiicrypt.exe",
                    Arguments = $"-c \"{tmpProvider.FilePath}\" {tmpProvider.Size:x8} write",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            encrypter.Start();
            Task<string> erroMessage = encrypter.StandardError.ReadToEndAsync();
            Task<string> outputMessage = encrypter.StandardOutput.ReadToEndAsync();
            encrypter.WaitForExit();
            if (encrypter.ExitCode != -2)
            {
                StringBuilder sb = new StringBuilder("Checksum error! Code: ");
                sb.AppendLine(encrypter.ExitCode.ToString());
                sb.AppendLine("Error: ");
                sb.AppendLine(erroMessage.Result);
                sb.AppendLine("Output: ");
                sb.AppendLine(outputMessage.Result);

                throw new InvalidDataException(sb.ToString());
            }
        }
    }
}