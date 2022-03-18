using System;
using System.IO;

namespace Pulse.Core
{
    public interface ISequencedStreamFactory
    {
        bool TryCreateNextStream(string key, out Stream result, out Exception ex);
    }
}