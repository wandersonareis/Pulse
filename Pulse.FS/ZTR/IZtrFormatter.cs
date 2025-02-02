﻿using System.Collections.Generic;
using System.IO;

namespace Pulse.FS
{
    public interface IZtrFormatter
    {
        void Write(StreamWriter sw, ZtrFileEntry entry, int index);
        void Write(StreamWriter sw, KeyValuePair<string, string> entry, int index);
        ZtrFileEntry Read(StreamReader sr, out int index);
    }
}