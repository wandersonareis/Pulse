﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrTextReader
    {
        private readonly Stream _input;
        private readonly IZtrFormatter _formatter;

        public ZtrTextReader(Stream input, IZtrFormatter formatter)
        {
            _input = input;
            _formatter = formatter;
        }

        public ZtrFileEntry[] Read(out string name)
        {
            using (StreamReader sr = new(_input, Encoding.UTF8, true, 4096, false))
            {
                name = sr.ReadLine();
                if (_formatter is StringsZtrFormatter) // TEMP
                    name = name?.Substring(2, name.Length - 4);

                string countStr = sr.ReadLine();
                if (_formatter is StringsZtrFormatter) // TEMP
                    countStr = countStr?.Substring(2, countStr.Length - 4);
                Debug.Assert(countStr != null, nameof(countStr) + " != null");
                int count = int.Parse(countStr, CultureInfo.InvariantCulture);
                List<ZtrFileEntry> result = new(count);

                for (var i = 0; i < count && !sr.EndOfStream; i++)
                {
                    ZtrFileEntry entry = _formatter.Read(sr, out int index);
                    if (entry == null)
                        continue;
                    var vi = Regex.Escape(entry.Value);
                    if (entry.Key.Equals("$f_con_credit"))
                    {
                        File.WriteAllText(@".\tabthing.txt", entry.Value);
                        File.WriteAllText(@".\tabEscape.txt", Regex.Escape(entry.Value));
                        File.WriteAllText(@".\tabUnescape.txt", Regex.Unescape(entry.Value));

                    }
                    if (entry.Key.Equals("$xdiskchg_err"))
                    {
                        File.AppendAllText(@".\tabthing.txt", entry.Value);
                        File.AppendAllText(@".\tabEscape.txt", Regex.Escape(entry.Value));
                        File.AppendAllText(@".\tabUnescape.txt", Regex.Unescape(entry.Value));
                    }

                    if (entry.Key.Equals("$ask_end_title"))
                    {
                        File.AppendAllText(@".\tabthing.txt", entry.Value);
                        File.AppendAllText(@".\tabEscape.txt", Regex.Escape(entry.Value));
                        File.AppendAllText(@".\tabUnescape.txt", Regex.Unescape(entry.Value));
                    }
                    //use to avoid empty keys
                    //if (string.IsNullOrEmpty(entry.Value))
                    //    continue;
                    if (string.IsNullOrWhiteSpace(entry.Key))
                    {
                        Log.Warning("Incorrect line: {0} entry [Key: {1}, Value: {2}] on file: {3}", result.Count - 1, entry.Key, entry.Value, name);
                        continue;
                    }
                    
                    result.Add(entry);
                }

                if (result.Count != count)
                    Log.Warning("Incorrect number of lines in the file: {0} from {1}", result.Count, count);
                sr.Close();
                return result.ToArray();
            }
        }
    }
}