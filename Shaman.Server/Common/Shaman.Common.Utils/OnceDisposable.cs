using System;
using System.Threading;

namespace Shaman.Common.Utils
{
    public abstract class OnceDisposable : IDisposable
    {
        private int _disposed = 0;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                DisposeImpl();
            }
        }

        protected abstract void DisposeImpl();
    }
}