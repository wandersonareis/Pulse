using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Meziantou.Framework;
using Pulse.Core;
using Pulse.FS;
using Yusnaan.Compressor;

namespace Yusnaan.Model.ztr
{
    internal class ZtrFileReader
    {
        public void PulseUnpack(string file, Stream input, Stream output)
        {
            ZtrFileUnpacker unPacker = new(input, FFXIIITextEncodingFactory.CreateEuro());
            ZtrFileEntry[] entries = unPacker.Unpack();

            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
            writer.Write(file, entries);
        }

        public void ToTxt(string file, string fileName, int encoding)
        {
            ZtrFileHandler unpack = new();

            string[] result = unpack.Decompressor(file, encoding);
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(file) ?? throw new InvalidOperationException(), $"{fileName}.txt"), result);
        }
        public async ValueTask ToStrings([DisallowNull] FullPath? file)
        {
            ArgumentNullException.ThrowIfNull(file);
            
            string? fileName = file.Value.NameWithoutExtension;
            string? name = fileName?.Split((char)46)[0];
            string? lang = name?[^3..];

            if (!GameEncoding.EncodingCode.TryGetValue(lang ?? throw new InvalidOperationException(), out int encodingCode)) encodingCode = 65001;

            ZtrFileHandler unpack = new();

            Dictionary<string, string> entries = unpack.DecompressorDict(file.Value.Value, encodingCode);

            await using FileStream output = File.Create(Path.ChangeExtension(file.Value.Value, ".strings")!);
            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
            writer.Write(fileName, entries);
        }
        public async ValueTask ZtrTextReader(FileInfo file)
        {
            ArgumentNullException.ThrowIfNull(file);

            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            string name = fileName.Split((char)46)[0];
            string? lang = name[^3..];

            if (!GameEncoding.EncodingCode.TryGetValue(lang ?? throw new InvalidOperationException(), out int encodingCode)) encodingCode = 65001;

            ZtrFileHandler unpack = new();

            Dictionary<string, string> entries = unpack.DecompressorDict(file.FullName, encodingCode);

            await using FileStream output = new(Path.ChangeExtension(file.FullName, ".strings"), FileMode.Create, FileAccess.ReadWrite);

            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
            writer.Write(fileName, entries);
        }

    }
}
