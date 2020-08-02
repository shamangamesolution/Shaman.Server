using Shaman.Contract.Common.Logging;

namespace Shaman.Contract.Bundle
{
    public interface IShamanComponents
    {
        IShamanLogger Logger { get; }
        IBackendProvider BackendProvider { get; }
    }
}