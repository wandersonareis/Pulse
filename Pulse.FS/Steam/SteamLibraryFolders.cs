using System;
using System.Collections.Generic;
using System.IO;

namespace Pulse.FS
{
    public static class SteamLibraryFolders
    {
        public static string[] Read(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return Array.Empty<string>();

            SteamKeyValues root = SteamKeyValues.Parse(File.ReadAllText(path));

            List<string> result = new();
            HashSet<string> unique = new(StringComparer.OrdinalIgnoreCase);

            Collect(root, result, unique);

            return result.ToArray();
        }

        private static void Collect(SteamKeyValues node, List<string> result, HashSet<string> unique)
        {
            foreach (SteamKeyValues child in node.Children)
            {
                string path = child.GetValue("path");

                if (!string.IsNullOrEmpty(path))
                {
                    string normalized = SteamPath.Normalize(path);
                    if (normalized != null && unique.Add(normalized))
                        result.Add(normalized);

                    continue;
                }

                Collect(child, result, unique);
            }
        }
    }
}
