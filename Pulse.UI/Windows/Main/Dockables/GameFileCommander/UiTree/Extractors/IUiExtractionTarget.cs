using Pulse.Core;

namespace Pulse.UI
{
    public interface IUiExtractionTarget
    {
        StreamSequence Create(string targetPath);
        void CreateDirectory(string directoryPath);
    }
}