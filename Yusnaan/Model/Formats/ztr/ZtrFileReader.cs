﻿using System.Diagnostics.CodeAnalysis;
using Meziantou.Framework;
using Pulse.Core;
using Pulse.FS;
using Yusnaan.Compressor;
using FileEx = Yusnaan.Common.FileEx;

namespace Yusnaan.Model.ztr;

internal sealed class ZtrFileReader
{
    private readonly Stream _input;
    private readonly Stream _output;

    public ZtrFileReader()
    {
        
    }

    public ZtrFileReader(Stream input, Stream output)
    {
        _input = input;
        _output = output;
    }
    public void PulseUnpack(string file)
    {
        ZtrFileUnpacker unPacker = new(_input, FFXIIITextEncodingFactory.CreateEuro());
        ZtrFileEntry[] entries = unPacker.Unpack();

        ZtrTextWriter writer = new(_output, StringsZtrFormatter.Instance);
        writer.Write(file, entries);
    }

    public void ToTxt(string file, string fileName, int encoding)
    {
        ZtrFileHandler unpack = new();

        string[] result = unpack.Decompressor(file, encoding);
        File.WriteAllLines(Path.Combine(Path.GetDirectoryName(file) ?? throw new InvalidOperationException(), $"{fileName}.txt"), result);
    }

    public async ValueTask ZtrTextReader(FullPath? file, string outputStrings = "")
    {
        ArgumentNullException.ThrowIfNull(file);
            
        string? fileName = file.Value.NameWithoutExtension;
        string? name = fileName?.Split((char)46)[0];
        string? lang = name?[^3..];

        if (!GameEncoding.EncodingCode.TryGetValue(lang ?? throw new InvalidOperationException(), out int encodingCode)) encodingCode = 65001;

        ZtrFileHandler unpack = new();

        Dictionary<string, string> entries = unpack.DecompressorDict(file.Value.Value, encodingCode);

        string stringsFile = string.IsNullOrEmpty(outputStrings) ? Path.ChangeExtension(file.Value.Value, ".strings") : outputStrings;
        FileStream output = new(stringsFile, FileEx.FileStreamOutputOptions());

        await using (output.ConfigureAwait(false))
        {
            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
            writer.Write(fileName, entries);
        }
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

        FileStream output = new(Path.ChangeExtension(file.FullName, ".strings"), FileEx.FileStreamOutputOptions());
        await using (output.ConfigureAwait(false))
        {
            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
        writer.Write(fileName, entries);
        output.Close();
        }
    }

}