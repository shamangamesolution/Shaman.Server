using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.TestTools.ClientPeers
{
    public class TestClientPeerFactory
    {
        private static readonly IShamanLogger ClientLogger =
            new ConsoleLogger("C ", LogLevel.Error);
        private static readonly IShamanLogger ServerLogger = new ConsoleLogger("S ", LogLevel.Error);
        private static readonly TaskSchedulerFactory TaskSchedulerFactory = new TaskSchedulerFactory(ServerLogger);
        public static TestClientPeer CreateDefault()
        {
            return new TestClientPeer(ClientLogger, TaskSchedulerFactory);

        }
    }
}