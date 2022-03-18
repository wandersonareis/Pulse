using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;
using Pulse.FS;
using Pulse.Gui.Compressor;

namespace Pulse.Gui.Common
{
    internal static class ZtrToStrings
    {
        public static void PulseUnpack(string file, Stream input, Stream output)
        {
            ZtrFileUnpacker unPacker = new(input, FFXIIITextEncodingFactory.CreateEuro());
            ZtrFileEntry[] entries = unPacker.Unpack();

            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
            writer.Write(file, entries);
        }

        public static void ToTxt(string file, string fileName, int encoding)
        {
            ZtrFileHandler unpack = new();

            string[] result = unpack.Decompressor(file, encoding);
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(file) ?? throw new InvalidOperationException(), $"{fileName}.txt"), result);
        }
        public static void ToStrings(string file, string fileName, int encoding)
        {
            ZtrFileHandler unpack = new();

            Dictionary<string, string> entries = unpack.DecompressorDict(file, encoding);

            using FileStream output = File.Create(Path.ChangeExtension(file, ".strings"));
            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
            writer.Write(fileName, entries);
        }

    }
}
