using System;

namespace Shaman.Common.Contract
{
    public interface IPendingTask : IDisposable
    {
        bool IsCompleted { get; }
    }
}