﻿using System.Globalization;
using System.Linq;

namespace Yusnaan.Compressor;

public static class CompressionLevel
{
    private static readonly List<byte[]> _Default = new()
    { 
        new byte[] { 0x0, 0x0 },
        /*new byte[] { 0x0, 0x1 },
        new byte[] { 0x2E, 0x20 },
        new byte[] { 0x2C, 0x20 },
        new byte[] { 0x21, 0x20 },
        new byte[] { 0x3F, 0x20 },
        new byte[] { 0x40, 0x72 },
        new byte[] { 0x21, 0x3F },*/
    };
    public static Dictionary<byte, byte[]> Default(ref byte[] unusedBytes, ref int unusedBytesIndex)
    {
        Dictionary<byte, byte[]> dict = new();
        for (int i = 0; i < unusedBytes.Length && i < _Default.Count; i++)
        {
            dict.Add(unusedBytes[unusedBytesIndex++], _Default[i]);
        }
        return dict;
    }
    public static bool Increase(ref Dictionary<byte, byte[]> dict, byte[] input, ref byte[] unusedBytes, ref int unusedBytesIndex)
    {
        byte[] value = GetDictionaryValue(input);
        if (unusedBytes.Length < 1 || value == null || dict.Values.Any(v => v.SequenceEqual(value))) return false;
        dict.Add(unusedBytes[unusedBytesIndex++], value);
        return true;
    }
    private static byte[] GetDictionaryValue(byte[] data)
    {
        int len = data.Length - 1;
        if (len < 1) throw new ArgumentNullException(nameof(data), @"len < 1");
        int max = 0;
        int found = 0;
        int[] dict = new int[ushort.MaxValue + 1];
        for (int i = 0; i < len; i++)
        {
            int num = data[i] | (data[i + 1] << 8);
            int cur = ++dict[num];
            if (cur > max)
            {
                max = cur;
                found = num;
            }
        }
        string hex = found != 0 ? found.ToString("X2", CultureInfo.InvariantCulture) : "0000";
        byte firstByte =
            byte.Parse(hex.Length == 2 ? hex.Substring(1, hex.Length - 1) : hex.Substring(2, hex.Length - 2),
                NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte lastByte =
            byte.Parse(hex.Length == 2 ? hex.Substring(0, hex.Length - 1) : hex.Substring(0, hex.Length - 2),
                NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return new byte[] { firstByte, lastByte };
    }
}