using System;

namespace Shaman.Contract.Common
{
    public interface IPendingTask : IDisposable
    {
        bool IsCompleted { get; }
    }
}