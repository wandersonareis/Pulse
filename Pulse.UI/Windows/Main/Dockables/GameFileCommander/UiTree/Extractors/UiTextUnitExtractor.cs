using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiTextUnitExtractor
    {
        private readonly ArchiveListing _listing;
        private readonly UiTextUnitLeaf[] _leafs;
        private readonly IUiExtractionTarget _target;

        public UiTextUnitExtractor(ArchiveListing listing, UiTextUnitLeaf[] leafs, IUiExtractionTarget target)
        {
            _listing = listing;
            _leafs = leafs;
            _target = target;
        }

        public void Extract()
        {
            string root = InteractionService.WorkingLocation.Provide().ProvideExtractedDirectory();

            foreach (UiTextUnitLeaf leaf in _leafs)
            {
                ArchiveEntry entry = leaf.Entry;

                string targetPath = Path.Combine(root, PathEx.ChangeMultiDotExtension(entry.Name, ".strings"));
                string directoryPath = Path.GetDirectoryName(targetPath);
                _target.CreateDirectory(directoryPath);

                using (Stream input = _listing.Accessor.ExtractBinary(entry))
                using (StreamSequence output = _target.Create(targetPath))
                {
                    byte[] container = ReadAllBytes(input);

                    using (MemoryStream source = new(container))
                    {
                        byte[] text = ZtrContainer.ReadText(source, out ZtrContainerType _);

                        using (MemoryStream input2 = new(text))
                        {
                            ZtrFileUnpacker unpacker = new(input2, InteractionService.TextEncoding.Provide().Encoding);
                            ZtrFileEntry[] entries = unpacker.Unpack();

                            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
                            writer.Write(entry.Name, entries);
                        }
                    }
                }
            }
        }

        internal static byte[] ReadAllBytes(Stream input)
        {
            using (MemoryStream ms = new())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
