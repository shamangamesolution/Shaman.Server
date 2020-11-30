using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Client.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Monkeys;
using Shaman.TestTools.Monkeys;

namespace Monkeys
{
    class Program
    {
        class MonkeyBehaviour : IMonkeyBehaviour
        {
            public Task Authenticate(Monkey monkey)
            {
                monkey.SessionId = Guid.NewGuid();
                return Task.CompletedTask;
            }

            public async Task Play(Monkey monkey)
            {
                await monkey.Peer.JoinGame(monkey.Route.MatchMakerAddress, monkey.Route.MatchMakerPort,
                    monkey.Route.BackendId,
                    monkey.SessionId, new Dictionary<byte, object>(), new Dictionary<byte, object>());
            }

            public IMonkeyFactory CreateMonkeyFactory(IShamanLogger logger, Options options)
            {
                return new MonkeyFactory(options.RouterUrl, options.ClientVersion, logger);
            }
        }

        class ClientPeerConfig : IShamanClientPeerConfig
        {
            public int PollPackageQueueIntervalMs => 20;
            public bool StartOtherThreadMessageProcessing => true;
            public int MaxPacketSize => 400;
            public int SendTickMs => 30;
        }

        class MonkeyFactory : MonkeyFactoryBase
        {
            public MonkeyFactory(string routerUrl, string clientVersion, IShamanLogger logger) : base(routerUrl,
                clientVersion,
                logger)
            {
            }

            protected override IShamanClientPeer CreateClientPeer(IRequestSender requestSender)
            {
                return new ShamanClientPeer(Logger, new TaskSchedulerFactory(Logger), new BinarySerializer(), requestSender,
                    null, new ClientPeerConfig());
            }
        }
        
        static void Main(string[] args)
        {
            MonkeyBootstrap.Launch(args, new MonkeyBehaviour());
        }
    }
}