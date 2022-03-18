using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yusnaan.Compressor;
using Pulse.Core;
using Pulse.FS;

namespace Yusnaan.Common
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
        public static void ToStrings(string? file)
        {
            string? fileName = Path.GetFileNameWithoutExtension(file);
            string? name = fileName?.Split((char)46).First();
            string? lang = name?[^3..];

            if (!GameEncoding.EncodingCode.TryGetValue(lang ?? throw new InvalidOperationException(), out int encodingCode)) encodingCode = 65001;

            ZtrFileHandler unpack = new();

            Dictionary<string, string> entries = unpack.DecompressorDict(file, encodingCode);

            using FileStream output = File.Create(Path.ChangeExtension(file, ".strings"));
            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
            writer.Write(fileName, entries);
        }

    }
}
