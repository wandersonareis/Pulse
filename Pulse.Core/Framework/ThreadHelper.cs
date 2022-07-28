using System.Threading;

namespace Pulse.Core
{
    public static class ThreadHelper
    {
        public static Thread StartBackground(string name, ThreadStart action)
        {
            Thread result = new(action) {Name = name, IsBackground = true};
            result.Start();
            return result;
        }
    }
}