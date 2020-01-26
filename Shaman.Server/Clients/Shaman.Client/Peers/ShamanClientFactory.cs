using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Client.Peers
{
    public class ShamanClientFactory
    {
        public static ShamanClientPeer Create(IRequestSender httpSender, IShamanClientPeerListener listener)
        {
            return new ShamanClientPeer(new ConsoleLogger(),
                new TaskSchedulerFactory(new ConsoleLogger()), 20, new BinarySerializer(), httpSender, listener);
        }
    }
}