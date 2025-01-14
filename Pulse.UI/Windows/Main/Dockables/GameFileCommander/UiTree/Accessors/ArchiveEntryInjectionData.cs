﻿using System;
using System.IO;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class ArchiveEntryInjectionData
    {
        public readonly ArchiveListing Listing;
        public readonly Func<ArchiveEntry, Stream> OuputStreamFactory;
        public readonly byte[] Buffer = new byte[32 * 1024];

        public ArchiveEntryInjectionData(ArchiveListing listing, Func<ArchiveEntry, Stream> ouputStreamFactory)
        {
            Listing = listing;
            OuputStreamFactory = ouputStreamFactory;
        }
    }
}