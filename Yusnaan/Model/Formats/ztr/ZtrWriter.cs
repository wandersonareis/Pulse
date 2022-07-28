using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pulse.Core;
using Pulse.FS;
using SimpleLogger;
using Yusnaan.Common;
using Yusnaan.Compressor;

namespace Yusnaan.Model.ztr
{
    internal class ZtrFileCompressorWriter
    {
        private readonly FileStream _stream;
        private readonly string _ztrFile;

        public ZtrFileCompressorWriter(FileStream stringsStream, string ztrFile)
        {
            _stream = stringsStream;
            _ztrFile = ztrFile;
        }
        public async ValueTask PackStringsNewCompression()
        {
            ZtrFileHandler ztrFileHandler = new();
            ZtrTextReader reader = new(_stream, StringsZtrFormatter.Instance);
            Dictionary<string, string> entries = reader.Read(out string name).ToDictionary(e => e.Key, e => e.Value, StringComparer.OrdinalIgnoreCase);

            string? ztr = Dialogs.GetZtrFile(name, _ztrFile);
            if (ztr == null) return;
            
            Logger.Log<ZtrFileCompressorWriter>(Logger.Level.Info, $"Opening file: {_stream.Name}");
            Logger.Log<ZtrFileCompressorWriter>(Logger.Level.Info, $"String lines: {entries.Count}");
            Logger.Log<ZtrFileCompressorWriter>(Logger.Level.Info, $"Write on file: {_ztrFile}");

            byte[] result = ztrFileHandler.Compressor(ztr, entries);
            await File.WriteAllBytesAsync(ztr, result);

            if (!TaskDialogs.skip)
            {
                new TaskDialogs().ShowTaskDialog("Task completed!", $"File {Path.GetFileNameWithoutExtension(ztr)} Wrote!", "Open folder in file manager?", ztr);
            }
        }

        public void StringsToZtrPulseEncodingNewCompression(FileStream fs, string file)
        {
            ZtrFileHandler ztrFileHandler = new();
            string ztr = Path.ChangeExtension(file, ".ztr");
            if (!File.Exists(ztr))
            {
                ztr = Dialogs.GetFile("Get Ztr File", "Ztr File|*.ztr") ?? throw new FileNotFoundException();
            }
            ZtrTextReader reader = new(fs, StringsZtrFormatter.Instance);
            Dictionary<string, string> entries = reader.Read(out string name).ToDictionary(e => e.Key, e => e.Value, StringComparer.Ordinal);
            byte[] result = ztrFileHandler.PulseEncodingCompressor(ztr, entries);
            File.WriteAllBytes(ztr, result);
        }
    }
}
