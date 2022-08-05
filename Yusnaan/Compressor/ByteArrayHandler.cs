namespace Yusnaan.Compressor;

public static class ByteArrayHandler
{
    private static int Search(byte[] src, byte[] pattern)
    {
        int maxFirstCharSlot = src.Length - pattern.Length + 1;
        for (int i = 0; i < maxFirstCharSlot; i++)
        {
            if (src[i] != pattern[0]) // compare only first byte
                continue;

            // found a match on first byte, now try to match rest of the pattern
            for (int j = pattern.Length - 1; j >= 1; j--)
            {
                if (src[i + j] != pattern[j]) break;
                if (j == 1) return i;
            }
        }
        return -1;
    }

    private static int FindBytes(byte[] src, byte[] find)
    {
        int index = -1;
        int matchIndex = 0;
        for (int i = 0; i < src.Length; i++)
        {
            if (src[i] == find[matchIndex])
            {
                if (matchIndex == (find.Length - 1))
                {
                    index = i - matchIndex;
                    break;
                }
                matchIndex++;
            }
            else if (src[i] == find[0])
            {
                matchIndex = 1;
            }
            else
            {
                matchIndex = 0;
            }

        }
        return index;
    }
#nullable disable
    public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
    {
        byte[] dst = null;
        byte[] temp = null;
        int index = FindBytes(src, search);
        //int index = Search(src, search);
        while (index >= 0)
        {
            temp = temp == null ? src : dst;

            dst = new byte[temp.Length - search.Length + repl.Length];

            Buffer.BlockCopy(temp, 0, dst, 0, index);
            Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
            Buffer.BlockCopy(
                temp,
                index + search.Length,
                dst,
                index + repl.Length,
                temp.Length - (index + search.Length));

            index = FindBytes(dst, search);
            //index = Search(dst, search);
        }
        return dst;
    }
#nullable enable
}