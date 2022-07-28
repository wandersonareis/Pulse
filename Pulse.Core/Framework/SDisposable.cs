using System;
using System.Collections.Concurrent;

namespace Pulse.Core
{
    public static class DisposableSerivce
    {
        private static readonly ConcurrentStack<IDisposable> Disposables = new();

        public static void Register(IDisposable disposable)
        {
            Disposables.Push(disposable);
        }

        public static void DisposeAll()
        {
            IDisposable disposable;
            while (Disposables.TryPop(out disposable))
                disposable.SafeDispose();
        }
    }
}