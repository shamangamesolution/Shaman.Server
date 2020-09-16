using System;
using System.Linq;
using Newtonsoft.Json;
using Shaman.Client;
using Shaman.Client.Peers;
using Shaman.Client.Providers;
using Shaman.Common.Http;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.Balancing;
using Shaman.Serialization;
using Shaman.TestTools.ClientPeers;
using IRequestSender = Shaman.Client.IRequestSender;

namespace Shaman.TestTools.Monkeys
{
    public abstract class MonkeyFactoryBase : IMonkeyFactory
    {
        protected readonly IShamanLogger Logger;
        private readonly Route _route;
        private readonly TestClientHttpSender _requestSender;
        
        protected MonkeyFactoryBase(string routerUrl, string clientVersion, IShamanLogger logger)
        {
            Logger = logger;
            _requestSender = new TestClientHttpSender(logger, new BinarySerializer());
            var routerClient = new TestRouterClient(_requestSender, logger, routerUrl);
            var clientServerInfoProvider =
                new ClientServerInfoProvider(logger, routerClient);

            _route = clientServerInfoProvider.GetRoutes(routerUrl, clientVersion).Result.First();
            logger.Debug($"Using route: {JsonConvert.SerializeObject(_route, Formatting.Indented)}");
        }

        public Monkey Create()
        {
            return new Monkey
            {
                Peer = CreateClientPeer(_requestSender),
                Route = _route,
                GuestId = Guid.NewGuid().ToString()
            };
        }

        protected abstract IShamanClientPeer CreateClientPeer(IRequestSender requestSender);
    }
}