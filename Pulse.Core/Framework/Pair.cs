namespace Pulse.Core
{
    public sealed class Pair<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public bool IsAnyEmpty => Item1 == null || Item2 == null;
    }
}