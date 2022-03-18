using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Core;
using Yusnaan.Compressor;
using Pulse.FS;
using Pulse.UI;

namespace Yusnaan.Common
{
    internal static class Packer
    {
        public static void PackStringsNewCompression(FileStream stringsStream, string ztrFile)
        {
            ZtrFileHandler ztrFileHandler = new();
            ZtrTextReader reader = new(stringsStream, StringsZtrFormatter.Instance);
            Dictionary<string, string> entries = reader.Read(out string name).ToDictionary(e => e.Key, e => e.Value);

            string? ztr = Dialogs.GetZtrFile(name, ztrFile);
            if (ztr == null) return;

            byte[] result = ztrFileHandler.Compressor(ztr, entries);
            File.WriteAllBytes(ztr, result);
        }

        public static void StringsToZtrPulseEncodingNewCompression(FileStream fs, string file)
        {
            ZtrFileHandler ztrFileHandler = new();
            string ztr = Path.ChangeExtension(file, ".ztr");
            if (!File.Exists(ztr))
            {
                ztr = Dialogs.GetFile("Get Ztr File", "Ztr File|*.ztr") ?? throw new FileNotFoundException();
            }
            ZtrTextReader reader = new(fs, StringsZtrFormatter.Instance);
            Dictionary<string, string> entries = reader.Read(out string name).ToDictionary(e => e.Key, e => e.Value);
            byte[] result = ztrFileHandler.PulseEncodingCompressor(ztr, entries);
            File.WriteAllBytes(ztr, result);
        }

        public static void PackStringsWithPulse(FileStream fs, string fi)
        {
            using FileStream output = File.Create(Path.ChangeExtension(fi, ".ztr"));
            ZtrTextReader reader = new(fs, StringsZtrFormatter.Instance);
            ZtrFileEntry[] entries = reader.Read(out string name);

            ZtrFilePacker packer = new(output, FFXIIITextEncodingFactory.CreateEuro(), ZtrFileType.BigEndianCompressedDictionary);
            packer.Pack(entries);
        }
    }
}
