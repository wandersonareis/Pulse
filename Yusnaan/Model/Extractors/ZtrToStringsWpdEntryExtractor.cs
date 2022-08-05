using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.FS;
using Yusnaan.Compressor;

namespace Yusnaan.Model.Extractors;

internal class ZtrToStringsWpdEntryExtractor
{
    public static void ToStrings(Stream stream, string? file)
    {
        string? fileName = Path.GetFileNameWithoutExtension(file);
        string? name = fileName?.Split((char)46).First();
        string? lang = name?[^3..];

        if (!GameEncoding.EncodingCode.TryGetValue(lang ?? string.Empty, out int encodingCode)) encodingCode = 65001;

        ZtrFileHandler unpack = new();

        Dictionary<string, string> entries = unpack.DecompressorDict(stream, encodingCode);

        using FileStream output = File.Create(Path.ChangeExtension(file, ".strings") ?? throw new InvalidOperationException());
        ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
        writer.Write(fileName, entries);
    }
}