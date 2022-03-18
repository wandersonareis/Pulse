using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
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
            using (StreamReader sr = new StreamReader(_input, Encoding.UTF8, true, 4096, false))
            {
                name = sr.ReadLine();
                if (_formatter is StringsZtrFormatter) // TEMP
                    name = name?.Substring(2, name.Length - 4);

                string countStr = sr.ReadLine();
                if (_formatter is StringsZtrFormatter) // TEMP
                    countStr = countStr?.Substring(2, countStr.Length - 4);
                Debug.Assert(countStr != null, nameof(countStr) + " != null");
                int count = int.Parse(countStr, CultureInfo.InvariantCulture);
                List<ZtrFileEntry> result = new List<ZtrFileEntry>(count);

                for (var i = 0; i < count && !sr.EndOfStream; i++)
                {
                    ZtrFileEntry entry = _formatter.Read(sr, out int index);
                    if (entry == null)
                        continue;
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