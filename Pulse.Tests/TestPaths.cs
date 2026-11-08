using System;
using System.IO;

namespace Pulse.Tests
{
    public static class TestPaths
    {
        public const string LooseZtr = @"F:\ff13crypt\txtres_us_original.ztr";
        public const string ZtrContainer = @"F:\ff-resources\apps\ff13\bin\data\mods\part3\db\ai\npc\pack\auto_c01_us.bin";

        public static bool Exists(string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        public static string CopyToTemp(string source)
        {
            string target = Path.Combine(Path.GetTempPath(), "pulse-tests", Guid.NewGuid().ToString("N") + Path.GetExtension(source));

            string directory = Path.GetDirectoryName(target);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.Copy(source, target, true);
            return target;
        }
    }
}
