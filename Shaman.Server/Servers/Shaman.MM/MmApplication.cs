using System;
using System.Linq;
using Shaman.Common.Http;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Protection;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle.Stats;
using Shaman.Contract.Common.Logging;
using Shaman.Contract.Routing.MM;
using Shaman.MM.MatchMaking;
using Shaman.MM.Peers;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.Serialization;

namespace Shaman.MM
{
    public class MmApplication : ApplicationBase<MmPeerListener, MmPeer>
    {
        private readonly IMatchMaker _matchMaker;
        private readonly IPacketSender _packetSender;
        private readonly IShamanMessageSenderFactory _messageSenderFactory;
        private readonly IPlayersManager _playersManager;
        private readonly IMatchMakerServerInfoProvider _serverProvider;
        private readonly IRoomManager _roomManager;
        private readonly IMatchMakingGroupsManager _matchMakingGroupManager;

        //debug
        private readonly Guid _id;

        public MmApplication(
            IShamanLogger logger,
            IApplicationConfig config,
            ISerializer serializer,
            ISocketFactory socketFactory,
            IMatchMaker matchMaker,
            IRequestSender requestSender,
            ITaskSchedulerFactory taskSchedulerFactory,
            IPacketSender packetSender, 
            IShamanMessageSenderFactory messageSenderFactory,
            IMatchMakerServerInfoProvider serverProvider,
            IRoomManager roomManager, IMatchMakingGroupsManager matchMakingGroupManager, IPlayersManager playersManager, IMmMetrics mmMetrics, IProtectionManager protectionManager) : base(logger, config, serializer,
            socketFactory, taskSchedulerFactory, requestSender, mmMetrics, protectionManager)
        {
            _packetSender = packetSender;
            _messageSenderFactory = messageSenderFactory;
            _serverProvider = serverProvider;
            _roomManager = roomManager;
            _matchMakingGroupManager = matchMakingGroupManager;
            _playersManager = playersManager;
            _matchMaker = matchMaker;
            _id = Guid.NewGuid();

            Logger?.Debug($"MmApplication constructor called. Id = {_id}");
        }

        public MmServerStats GetStats()
        {
            var oldestPlayer = _playersManager.GetOldestPlayer();
            return new MmServerStats
            {
                RegisteredServers = _serverProvider.GetGameServers().Select(s => new RegisteredServerStat
                {
                    ActualizedGap = s.ActualizedGap,
                    Address = s.Identity.ToString()
                }).ToList(),
                TotalPlayers = _playersManager.Count(),
                OldestPlayerInMatchMaking = oldestPlayer?.StartedOn,
                CreatedRoomsCount = _roomManager.GetRoomsCount()
            };
        }
        
        public override void OnStart()
        {
            _packetSender.Start(false);
            
            _matchMaker.Start();
            var listeners = GetListeners();
            var shamanMessageSender = _messageSenderFactory.Create(_packetSender);
            foreach (var listener in listeners)
            {
                listener.Initialize(_matchMaker, shamanMessageSender, _roomManager, _matchMakingGroupManager, Config.AuthSecret);
            }
        }

        protected override void TrackMetrics()
        {
            base.TrackMetrics();
            ServerMetrics.TrackSendersCount(nameof(MmApplication), _packetSender.GetKnownPeersCount());
        }

        public override void OnShutDown()
        {
            _playersManager.Clear();
            _matchMaker.Stop();
        }
    }
}