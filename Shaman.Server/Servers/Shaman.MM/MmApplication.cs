using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Game.Contract.Stats;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.MM.Configuration;
using Shaman.MM.MatchMaking;
using Shaman.MM.Peers;
using Shaman.MM.Players;
// using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages.MM;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;

namespace Shaman.MM
{
    public class MmApplication : ApplicationBase<MmPeerListener, MmPeer>
    {
        private readonly IMatchMaker _matchMaker;
        private readonly IPacketSender _packetSender;
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
            IMatchMakerServerInfoProvider serverProvider,
            IRoomManager roomManager, IMatchMakingGroupsManager matchMakingGroupManager, IPlayersManager playersManager, IMmMetrics mmMetrics) : base(logger, config, serializer,
            socketFactory, taskSchedulerFactory, requestSender, mmMetrics)
        {
            _packetSender = packetSender;
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
                    ActualizedOn = s.ActualizedOn,
                    Address = s.Identity.ToString()
                }).ToList(),
                TotalPlayers = _playersManager.Count(),
                OldestPlayerInMatchMaking = oldestPlayer?.StartedOn,
                CreatedRoomsCount = _roomManager.GetRoomsCount()
            };
        }
        
        public override void OnStart()
        {
            var config = GetConfigAs<MmApplicationConfig>();
            
            _packetSender.Start(false);
            
            _matchMaker.Start();
            var listeners = GetListeners();
            foreach (var listener in listeners)
            {
                listener.Initialize(_matchMaker, _packetSender, _roomManager, _matchMakingGroupManager, Config.GetAuthSecret());
            }
        }

        public override void OnShutDown()
        {
            _playersManager.Clear();
            _matchMaker.Stop();
        }
    }
}