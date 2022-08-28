using System.Diagnostics;
using System.Linq;
using System.Text;
using Be.IO;
using Pulse.Core;

#pragma warning disable CS8600

namespace Yusnaan.Compressor;

public sealed class ZtrFileHandler
{
#nullable disable
    #region Structures
    private record Header
    {
        public int Magic;
        public int Version;
        public int TextCount;
        public int KeysDecompressedSize;
        public int TextBlocksCount;
        public int[] TextBlocksPointer;
    }
    private record TextInfo
    {
        public byte Block;
        public byte BlockOffset;
        public ushort CompressedPointer;
        public int Index;
        public byte[] Text;
    }
    private struct CompressDictionary
    {
        public int DictSize;
        public Dictionary<byte, byte[]> Dict;
    }
    #endregion
    private static Header ReadHeader(ref BeBinaryReader reader)
    {
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
        Header header = new()
        {
            Magic = reader.ReadInt32(),
            Version = reader.ReadInt32(),
            TextCount = reader.ReadInt32(),
            KeysDecompressedSize = reader.ReadInt32(),
            TextBlocksCount = reader.ReadInt32()
        };
        header.TextBlocksPointer = new int[header.TextBlocksCount];
        for (var i = 0; i < header.TextBlocksCount; i++)
        {
            header.TextBlocksPointer[i] = reader.ReadInt32();
        }
        return header;
    }
    private static TextInfo[] ReadTextInfos(ref BeBinaryReader reader, Header header)
    {
        reader.BaseStream.Seek(0x14 + header.TextBlocksCount * 4, SeekOrigin.Begin);
        var textInfos = new TextInfo[header.TextCount];
        for (var i = 0; i < textInfos.Length; i++)
        {
            textInfos[i] = new TextInfo
            {
                Block = reader.ReadByte(),
                BlockOffset = reader.ReadByte(),
                CompressedPointer = reader.ReadUInt16(),
                Index = i
            };
        }
        return textInfos;
    }
    private static CompressDictionary ReadDictionary(ref BeBinaryReader reader)
    {
        CompressDictionary dict = new()
        {
            DictSize = reader.ReadInt32(),
            Dict = new()
        };
        for (var i = 0; i < dict.DictSize / 3; i++)
        {
            List<byte> value = new();

            byte key = reader.ReadByte();
            byte valueFirst = reader.ReadByte();
            byte valueLast = reader.ReadByte();

            if (dict.Dict.TryGetValue(valueFirst, out byte[] valueFirstKey)) { value.AddRange(valueFirstKey); }
            else
            {
                value.Add(valueFirst);
            }

            if (dict.Dict.TryGetValue(valueLast, out byte[] valueLastKey)) { value.AddRange(valueLastKey); }
            else
            {
                value.Add(valueLast);
            }
            dict.Dict.Add(key, value.ToArray());
        }
        return dict;
    }
    private static string[] DecompressIDs(ref BeBinaryReader reader, Header header)
    {
        List<byte> totalBytes = new();
        var blockCount = (int)Math.Ceiling((double)header.KeysDecompressedSize / 4096);
        for (var i = 0; i < blockCount; i++)
        {
            CompressDictionary idsDict = ReadDictionary(ref reader);
            var blockLen = 0;
            while (totalBytes.Count < header.KeysDecompressedSize && blockLen < 4096)
            {
                byte entry = reader.ReadByte();
                if (idsDict.Dict.TryGetValue(entry, out byte[] compressed))
                {
                    totalBytes.AddRange(compressed);
                    blockLen += compressed.Length;
                }
                else
                {
                    totalBytes.Add(entry);
                    blockLen++;
                }
            }
        }
        string[] result = Encoding.ASCII.GetString(totalBytes.ToArray()).Split(new[] {(char)0});
        return result.Take(result.Length - 1).ToArray();
    }
    private static string[] DecompressText(ref BeBinaryReader reader, Header header, int encodingCode)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        List<byte> totalBytes = new();
        long pointer = 0;
        for (var i = 0; i < header.TextBlocksPointer.Length - 1; i++)
        {
            long endBlockPointer = header.TextBlocksPointer[i + 1];
            CompressDictionary textDict = ReadDictionary(ref reader);
            pointer += textDict.DictSize + 4;
            while (pointer < endBlockPointer)
            {
                byte entry = reader.ReadByte();
                if (textDict.Dict.TryGetValue(entry, out byte[] compressed)) totalBytes.AddRange(compressed);
                else totalBytes.Add(entry);
                pointer++;
            }
        }
        var result = new string[header.TextCount];
        Dictionary<string, byte[]> gameCode = GameEncoding.GameCode.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
        if (encodingCode == 65001) foreach (KeyValuePair<string, byte[]> entry in GameEncoding.JapaneseSymbol) gameCode.Add(entry.Key, entry.Value);
        var index = 0;
        for (var i = 0; i < result.Length; i++)
        {
            List<byte> textBytes = new();
            while (index < totalBytes.Count - 1)
            {
                if (totalBytes[index] == 0 && totalBytes[index + 1] == 0)
                {
                    index += 2;
                    break;
                }

                textBytes.Add(totalBytes[index++]);
            }
            foreach (byte[] temp in gameCode.Select(entry =>
                     {
                         (string key, byte[] value) = entry;
                         return ByteArrayHandler.ReplaceBytes(textBytes.ToArray(), value,
                             Encoding.UTF8.GetBytes(key));
                     }).Where(temp => temp != null))
            {
                textBytes = temp.ToList();
            }
            var encoding = Encoding.GetEncoding(encodingCode);
            result[i] = encoding.GetString(textBytes.ToArray());
            //Console.WriteLine(result[i]);
        }
        return result.ToArray();
    }
    public string[] Decompressor(string input, int encodingCode = 65001)
    {
        using FileStream stream = File.OpenRead(input);
        BeBinaryReader reader = new(stream);
        Header header = ReadHeader(ref reader);
        TextInfo[] textInfos = ReadTextInfos(ref reader, header);
        string[] idsDecompressed = DecompressIDs(ref reader, header);
        string[] textDecompressed = DecompressText(ref reader, header, encodingCode);
        var result = new string[idsDecompressed.Length];
        for (int i = result.Length - 1; i >= 0; i--)
        {
            result[i] = $"/*{idsDecompressed[i]}\n{textDecompressed[i]}\n*/";
        }
        return result;
    }

    public Dictionary<string, string> DecompressorDict(string? input, int encodingCode = 65001) =>
        DecompressorDict(File.OpenRead(input ?? throw new ArgumentNullException(nameof(input))), encodingCode);

    public Dictionary<string, string> DecompressorDict(Stream input, int encodingCode = 65001)
    {
        //using FileStream stream = File.OpenRead(input);
        BeBinaryReader reader = new(input);
        Header header = ReadHeader(ref reader);
        TextInfo[] textInfos = ReadTextInfos(ref reader, header);
        string[] idsDecompressed = DecompressIDs(ref reader, header);
        string[] textDecompressed = DecompressText(ref reader, header, encodingCode);
        //string[] result = new string[idsDecompressed.Length];
        Dictionary<string, string> dict = new(idsDecompressed.Length, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < idsDecompressed.Length; i++)
        {
            //result[i] = $"/*{idsDecompressed[i]}\n{textDecompressed[i]}\n*/";
            dict.Add(idsDecompressed[i], textDecompressed[i]);
        }
        return dict;
    }
        
    public byte[] Compressor(string ztr, IDictionary<string, string> input, int encodingCode = 65001)
    {
        MemoryStream result = new();

        const int maxBlockSize = 4096 * 4;
        //const int maxBlockSize = 4096;
        using (BeBinaryWriter writer = new(result))
        {
            using FileStream ztrStream = File.OpenRead(ztr);
            BeBinaryReader reader = new(ztrStream);
            Header header = ReadHeader(ref reader);
            TextInfo[] textInfos = ReadTextInfos(ref reader, header);
            long idsCompressedPointer = reader.BaseStream.Position;
            string[] idsDecompressed = DecompressIDs(ref reader, header);
            long textCompressedPointer = reader.BaseStream.Position;
            Dictionary<string, byte[]> gameCode = GameEncoding.GameCode.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);
            if (encodingCode == 65001) foreach (KeyValuePair<string, byte[]> entry in GameEncoding.JapaneseSymbol) gameCode.Add(entry.Key, entry.Value);
            for (var i = 0; i < idsDecompressed.Length; i++)
            {
                if (input.TryGetValue(idsDecompressed[i], out string value))
                {
                    byte[] bs = Encoding.GetEncoding(encodingCode).GetBytes(value);
                    foreach (byte[] temp in gameCode.Select(entry => ByteArrayHandler.ReplaceBytes(bs, Encoding.UTF8.GetBytes(entry.Key), entry.Value)).Where(temp => temp != null))
                    {
                        bs = temp;
                    }
                    var textBytes = new byte[bs.Length + 2];
                    bs.CopyTo(textBytes, 0);
                    textInfos[i].Text = textBytes;
                }
                else textInfos[i].Text = new byte[2];
            }
            reader.BaseStream.Position = idsCompressedPointer;
            byte[] idsCompressedData = reader.ReadBytes((int)(textCompressedPointer - idsCompressedPointer));

            var blockCount = 1;

            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            writer.Write(header.Magic);
            writer.Write(header.Version);
            writer.Write(header.TextCount);
            writer.Write(header.KeysDecompressedSize);
            writer.BaseStream.Position += 8;

            MemoryStream compressTextStream = new();
            BeBinaryWriter compressWr = new(compressTextStream);

            var textIndex = 0;
            var blockPointer = 0;

            while (blockCount <= 0xFF && textIndex < textInfos.Length - 1)
            {
                List<byte> uncompressedBlock = new();
                List<TextInfo> texts = new();
                int start = textIndex;
                while (uncompressedBlock.Count + textInfos[textIndex].Text.Length < maxBlockSize && textIndex < textInfos.Length - 1) //maximum capacity per block ?
                {
                    uncompressedBlock.AddRange(textInfos[textIndex].Text);
                    texts.Add(textInfos[textIndex]);
                    textIndex++;
                }
                int end = textIndex++;
                TextInfo[] textsSorted = texts.OrderByDescending(entry => entry.Text.Length).ToArray();
                byte[] unusedBytes = GetUnusedBytes(uncompressedBlock.ToArray());
                var unusedBytesIndex = 0;
                List<byte> compressedBlock = new();
                Dictionary<byte, byte[]> dict = CompressionLevel.Default(ref unusedBytes, ref unusedBytesIndex);
                foreach (TextInfo t in textsSorted)
                {
                    var optimized = false;
                    byte[] compressedText = t.Text;
                    foreach (byte[] temp in dict.Select(entry => ByteArrayHandler.ReplaceBytes(compressedText, entry.Value, new[] { entry.Key })).Where(temp => temp != null))
                    {
                        compressedText = temp;
                    }
                    while (compressedText.Length >= 2 && !optimized && unusedBytesIndex < unusedBytes.Length)
                    {
                        optimized = !CompressionLevel.Increase(ref dict, compressedText, ref unusedBytes, ref unusedBytesIndex);
                        if (!optimized)
                        {
                            byte[] temp = ByteArrayHandler.ReplaceBytes(compressedText, dict.Last().Value, new byte[] { dict.Last().Key });
                            if (temp != null) compressedText = temp;
                        }
                    }
                    TextInfo info = Array.Find(textInfos, entry => entry.Index == t.Index);
                    info.Text = compressedText;
                    info.Block = (byte)(blockCount - 1);
                }
                var textPointer = 0;
                for (int x = start; x <= end; x++)
                {
                    compressedBlock.AddRange(textInfos[x].Text);
                    textInfos[x].CompressedPointer = (ushort)textPointer;
                    textPointer += textInfos[x].Text.Length;
                }
                compressWr.Write(dict.Count * 3);
                foreach (KeyValuePair<byte, byte[]> entry in dict)
                {
                    compressWr.Write(entry.Key);
                    compressWr.Write(entry.Value);
                }
                compressWr.Write(compressedBlock.ToArray());
                blockPointer += (dict.Count * 3) + 4 + compressedBlock.Count;
                writer.Write(blockPointer);
                blockCount++;
            }
            compressWr.Close();
            for (var i = 0; i < header.TextCount; i++)
            {
                writer.Write(textInfos[i].Block);
                writer.BaseStream.Position += 1;
                writer.Write(textInfos[i].CompressedPointer);
            }
            writer.Write(idsCompressedData);
            writer.Write(compressTextStream.ToArray());
            writer.BaseStream.Position = 0x10;
            writer.Write(blockCount);
        }
        return result.ToArray();
    }

    public byte[] PulseEncodingCompressor(string ztr, IDictionary<string, string> input, int encodingCode = 65001)
    {
        MemoryStream result = new();
            
        //const int maxBlockSize = 4096 * 4;
        const int maxBlockSize = 4096;
        using BeBinaryWriter writer = new(result);
        using FileStream ztrStream = File.OpenRead(ztr);
        BeBinaryReader reader = new(ztrStream);
        Header header = ReadHeader(ref reader);
        TextInfo[] textInfos = ReadTextInfos(ref reader, header);
        long idsCompressedPointer = reader.BaseStream.Position;
        string[] idsDecompressed = DecompressIDs(ref reader, header);
        long textCompressedPointer = reader.BaseStream.Position;
        for (var i = 0; i < idsDecompressed.Length; i++)
        {
            if (input.TryGetValue(idsDecompressed[i], out string? value))
            {
                byte[] pulse = FFXIIITextEncodingFactory.CreateEuro().GetBytes(value ?? throw new InvalidOperationException());

                var textBytes = new byte[pulse.Length + 2];
                pulse.CopyTo(textBytes, 0);

                //string bss = Encoding.GetEncoding(65001).GetString(textBytes);
                //string pulses = FFXIIITextEncodingFactory.CreateEuro().GetString(textBytes);
                //if (bss.Contains("For"))
                //{

                //}

                textInfos[i].Text = textBytes;
            }
            else textInfos[i].Text = new byte[2];
        }
        reader.BaseStream.Position = idsCompressedPointer;
        byte[] idsCompressedData = reader.ReadBytes((int)(textCompressedPointer - idsCompressedPointer));

        var blockCount = 1;

        writer.BaseStream.Seek(0, SeekOrigin.Begin);
        writer.Write(header.Magic);
        writer.Write(header.Version);
        writer.Write(header.TextCount);
        writer.Write(header.KeysDecompressedSize);
        writer.BaseStream.Position += 8;

        MemoryStream compressTextStream = new();
        BeBinaryWriter compressWr = new(compressTextStream);

        var textIndex = 0;
        var blockPointer = 0;

        while (blockCount <= 0xFF && textIndex < textInfos.Length - 1)
        {
            List<byte> uncompressedBlock = new();
            List<TextInfo> texts = new();
            int start = textIndex;
            while (uncompressedBlock.Count + textInfos[textIndex].Text.Length < maxBlockSize && textIndex < textInfos.Length - 1) //maximum capacity per block ?
            {
                uncompressedBlock.AddRange(textInfos[textIndex].Text);
                texts.Add(textInfos[textIndex]);
                textIndex++;
            }
            int end = textIndex++;
            TextInfo[] textsSorted = texts.OrderByDescending(entry => entry.Text.Length).ToArray();
            byte[] unusedBytes = GetUnusedBytes(uncompressedBlock.ToArray());
            var unusedBytesIndex = 0;
            List<byte> compressedBlock = new();
            Dictionary<byte, byte[]> dict = CompressionLevel.Default(ref unusedBytes, ref unusedBytesIndex);
            foreach (TextInfo t in textsSorted)
            {
                var optimized = false;
                byte[] compressedText = t.Text;
                foreach (byte[] temp in dict.Select(entry =>
                             ByteArrayHandler.ReplaceBytes(compressedText, entry.Value, new[] { entry.Key })).Where(temp => temp != null))
                {
                    compressedText = temp;
                }
                while (compressedText.Length >= 2 && !optimized && unusedBytesIndex < unusedBytes.Length)
                {
                    optimized = !CompressionLevel.Increase(ref dict, compressedText, ref unusedBytes, ref unusedBytesIndex);
                    if (optimized) continue;
                    byte[] temp = ByteArrayHandler.ReplaceBytes(compressedText, dict.Last().Value, new[] { dict.Last().Key });
                    if (temp != null) compressedText = temp;
                }
                TextInfo info = Array.Find(textInfos, entry => entry.Index == t.Index);
                Debug.Assert(info != null, nameof(info) + " != null");
                info.Text = compressedText;
                info.Block = (byte)(blockCount - 1);
            }
            var textPointer = 0;
            for (int x = start; x <= end; x++)
            {
                compressedBlock.AddRange(textInfos[x].Text);
                textInfos[x].CompressedPointer = (ushort)textPointer;
                textPointer += textInfos[x].Text.Length;
            }
            compressWr.Write(dict.Count * 3);
            foreach ((byte key, byte[] value) in dict)
            {
                compressWr.Write(key);
                compressWr.Write(value);
            }
            compressWr.Write(compressedBlock.ToArray());
            blockPointer += (dict.Count * 3) + 4 + compressedBlock.Count;
            writer.Write(blockPointer);
            blockCount++;
        }
        compressWr.Close();
        for (var i = 0; i < header.TextCount; i++)
        {
            writer.Write(textInfos[i].Block);
            writer.BaseStream.Position += 1;
            writer.Write(textInfos[i].CompressedPointer);
        }
        writer.Write(idsCompressedData);
        writer.Write(compressTextStream.ToArray());
        writer.BaseStream.Position = 0x10;
        writer.Write(blockCount);

        return result.ToArray();
    }

    private static byte[] GetUnusedBytes(byte[] input)
    {
        List<byte> result = new();
        var find = 3;
        while (find < 0xFF)
        {
            if (!input.Contains(Convert.ToByte(find)))
            {
                result.Add(Convert.ToByte(find));
            }
            find++;
        }
        return result.ToArray();
    }
}