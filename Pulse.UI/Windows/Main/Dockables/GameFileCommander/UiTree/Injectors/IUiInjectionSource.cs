using System.Collections.Generic;
using System.IO;

namespace Pulse.UI
{
    public interface IUiInjectionSource
    {
        string ProvideRootDirectory();
        bool DirectoryIsExists(string directoryPath);
        Stream TryOpen(string sourcePath);
        Dictionary<string, string> TryProvideStrings();
    }
}