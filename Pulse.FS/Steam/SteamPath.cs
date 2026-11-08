using System;
using System.IO;

namespace Pulse.FS
{
    public static class SteamPath
    {
        public static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            string result = path.Trim().Trim('"').Replace('/', Path.DirectorySeparatorChar);

            try
            {
                result = Path.GetFullPath(result);
            }
            catch (Exception)
            {
                return null;
            }

            if (result.Length > 3)
                result = result.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return result;
        }
    }
}
