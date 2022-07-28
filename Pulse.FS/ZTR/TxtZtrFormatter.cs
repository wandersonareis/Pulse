using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Pulse.FS
{
    public sealed class TxtZtrFormatter : IZtrFormatter
    {
        private static readonly Lazy<TxtZtrFormatter> LazyInstance = new();

        public static TxtZtrFormatter Instance => LazyInstance.Value;

        public void Write(StreamWriter sw, ZtrFileEntry entry, int index)
        {
            sw.WriteLine("{0}║{1}║{2}", index.ToString("D4", CultureInfo.InvariantCulture), entry.Key, entry.Value);
        }
        public void Write(StreamWriter sw, KeyValuePair<string, string> entry, int index)
        {
            sw.WriteLine("{0}║{1}║{2}", index.ToString("D4", CultureInfo.InvariantCulture), entry.Key, entry.Value);
        }
        public ZtrFileEntry Read(StreamReader sr, out int index)
        {
            string str = sr.ReadLine();
            if (str == null)
            {
                index = -1;
                return null;
            }

            string[] line = str.Split('║');
            index = int.Parse(line[0], CultureInfo.InvariantCulture);
            return new() {Key = line[1], Value = line[2]};
        }

        
    }
}