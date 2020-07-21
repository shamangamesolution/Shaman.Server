using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Contract.Bundle
{
    public interface IShamanComponents
    {
        IRequestSender RequestSender { get; }
        IShamanLogger Logger { get; }
        IBackendProvider BackendProvider { get; }
        ISerializer Serializer { get; }
    }
}