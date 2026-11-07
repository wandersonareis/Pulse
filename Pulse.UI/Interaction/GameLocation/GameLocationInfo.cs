using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationInfo
    {
        public readonly string RootDirectory;
        public readonly string SystemDirectory;
        public readonly string MovieDirectory;
        public readonly string UpdatesDirectory;
        public readonly string AreasDirectory;
        public readonly string DlcDirectory;

        private const string Part1ResourceDirName = "white_data";
        private const string Part2ResourceDirName = "alba_data";
        private const string Part3ResourceDirName = "weiss_data";
        private const string ExecutableRelativePath = @"prog\win\bin\ffxiiiimg.exe";

        public GameLocationInfo(string rootDirectory)
        {
            RootDirectory = rootDirectory;

            string resourcePath = Path.Combine(RootDirectory, ResourceDirName);
            SystemDirectory = Path.Combine(resourcePath, "sys");
            MovieDirectory = Path.Combine(resourcePath, "movie");
            AreasDirectory = Path.Combine(resourcePath, "zone");
            UpdatesDirectory = Path.Combine(resourcePath, "udp");
            DlcDirectory = Path.Combine(resourcePath, "dlc");
        }

        public static string ResourceDirName
        {
            get
            {
                switch (InteractionService.GamePart)
                {
                    case FFXIIIGamePart.Part1:
                        return Part1ResourceDirName;
                    case FFXIIIGamePart.Part2:
                        return Part2ResourceDirName;
                    case FFXIIIGamePart.Part3:
                        return Part3ResourceDirName;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public string ExecutablePath => Path.Combine(RootDirectory, ResourceDirName, ExecutableRelativePath);

        public void Validate()
        {
            Exceptions.CheckDirectoryNotFoundException(SystemDirectory);
            Exceptions.CheckDirectoryNotFoundException(MovieDirectory);
            Exceptions.CheckDirectoryNotFoundException(AreasDirectory);
        }

        public void ToXml(XmlElement xmlElement)
        {
            xmlElement.SetString("RootDirectory", RootDirectory);
        }

        public static GameLocationInfo FromXml(XmlElement xmlElement)
        {
            if (xmlElement == null)
                return null;

            string rootDirectory = xmlElement.FindString("RootDirectory");
            return new(rootDirectory);
        }

        private readonly object _taskLock = new();
        private UiArchives _archives;
        private Task<UiArchives> _archivesBuilderTask;

        public Task<UiArchives> ArchivesTree
        {
            get
            {
                lock (_taskLock)
                {
                    if (_archives != null)
                        return Task.Run(() => _archives);
                    if (_archivesBuilderTask != null)
                        return _archivesBuilderTask;

                    lock (_taskLock)
                        return _archivesBuilderTask = Task.Run(() =>
                        {
                            var builder = new UiArchiveTreeBuilder(this);
                            lock (_taskLock)
                                _archives = builder.Build();
                            return _archives;
                        });
                }
            }
        }

        public IEnumerable<string> EnumerateListingFiless()
        {
            if (Directory.Exists(SystemDirectory))
                foreach (string path in Directory.EnumerateFiles(SystemDirectory, "filelist*.bin"))
                {
                    if (IsExcludedListing(path))
                        continue;

                    yield return path;
                }

            if (Directory.Exists(UpdatesDirectory))
                foreach (string path in Directory.EnumerateFiles(UpdatesDirectory, "filelist*.bin"))
                {
                    if (IsExcludedListing(path))
                        continue;

                    yield return path;
                }

            if (InteractionService.GamePart != FFXIIIGamePart.Part3)
                yield break;

            if (!Directory.Exists(DlcDirectory))
                yield break;

            foreach (string directory in Directory.EnumerateDirectories(DlcDirectory))
            {
                foreach (string path in Directory.EnumerateFiles(directory, "filelist*.bin"))
                {
                    if (IsExcludedListing(path))
                        continue;

                    yield return path;
                }
            }
        }

        private static bool IsExcludedListing(string path)
        {
            string name = Path.GetFileName(path);
            return string.Equals(name, "filelist.info.bin", StringComparison.OrdinalIgnoreCase);
        }
    }
}