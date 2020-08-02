using System;
using System.Linq;
using Newtonsoft.Json;
using Shaman.Client.Peers;
using Shaman.Client.Providers;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Contract.Common.Logging;
using Shaman.Router.Messages;
using Shaman.Serialization;

namespace Shaman.TestTools.Monkeys
{
    public abstract class MonkeyFactoryBase : IMonkeyFactory
    {
        protected readonly IShamanLogger Logger;
        private readonly Route _route;
        private readonly HttpSender _requestSender;

        protected MonkeyFactoryBase(string routerUrl, string clientVersion, IShamanLogger logger)
        {
            Logger = logger;
            _requestSender = new HttpSender(logger, new BinarySerializer());
            var clientServerInfoProvider =
                new ClientServerInfoProvider(_requestSender, logger);

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