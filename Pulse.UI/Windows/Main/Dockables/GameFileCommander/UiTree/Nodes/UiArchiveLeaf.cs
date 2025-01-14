﻿using System.Windows.Media;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveLeaf : UiNode, IUiLeaf
    {
        public ArchiveEntry Entry { get; }
        public ArchiveListing Listing { get; }

        public UiArchiveLeaf(string name, ArchiveEntry entry, ArchiveListing listing)
            : base(name, UiNodeType.ArchiveLeaf)
        {
            Entry = entry;
            Listing = listing;
        }

        public override ImageSource Icon
        {
            get
            {
                switch (PathEx.GetMultiDotComparableExtension(Entry.Name))
                {
                    case ".ztr":
                    case ".txt":
                        return Icons.TxtFileIcon;
                }
                return base.Icon;
            }
        }
    }
}