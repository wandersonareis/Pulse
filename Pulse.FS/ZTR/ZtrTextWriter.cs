using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Pulse.FS
{
    public sealed class ZtrTextWriter
    {
        private readonly Stream _output;
        private readonly IZtrFormatter _formatter;

        public ZtrTextWriter(Stream output, IZtrFormatter formatter)
        {
            _output = output;
            _formatter = formatter;
        }

        public void Write(string name, ZtrFileEntry[] entries)
        {
            using StreamWriter sw = new(_output, Encoding.UTF8, 4096, true);
            if (_formatter is StringsZtrFormatter) // TEMP
            {
                sw.WriteLine("/*" + name + "*/");
                sw.WriteLine("/*" + entries.Length.ToString("D4", CultureInfo.InvariantCulture) + "*/");
            }
            else
            {
                sw.WriteLine(name);
                sw.WriteLine(entries.Length.ToString("D4", CultureInfo.InvariantCulture));
            }

            for (int i = 0; i < entries.Length; i++)
                _formatter.Write(sw, entries[i], i);
        }
        public void Write(string name, Dictionary<string, string> entries)
        {
            using StreamWriter sw = new(_output, Encoding.UTF8, 4096, true);
            if (_formatter is StringsZtrFormatter) // TEMP
            {
                sw.WriteLine("/*" + name + "*/");
                sw.WriteLine("/*" + entries.Count.ToString("D4", CultureInfo.InvariantCulture) + "*/");
            }
            else
            {
                sw.WriteLine(name);
                sw.WriteLine(entries.Count.ToString("D4", CultureInfo.InvariantCulture));
            }

            foreach (var x in entries.Select((entry, index) => new {Entry = entry, Index = index }))
            {
                _formatter.Write(sw, x.Entry, x.Index);
                //Console.WriteLine("{0}: {1} = {2}", x.Index, x.Entry.Key, x.Entry.Value);
            }
        }
    }
}