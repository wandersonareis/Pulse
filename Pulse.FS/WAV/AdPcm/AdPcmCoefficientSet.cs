﻿using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class AdPcmCoefficientSet : IStreamingContent
    {
        public short X, Y;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new(stream);
            X = br.ReadInt16();
            Y = br.ReadInt16();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new(stream);
            bw.Write(X);
            bw.Write(Y);
        }
    }
}