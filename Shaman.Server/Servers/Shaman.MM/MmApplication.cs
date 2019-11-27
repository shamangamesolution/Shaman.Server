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
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages.MM;
using Shaman.MM.Providers;

namespace Shaman.MM
{
    public class MmApplication : ApplicationBase<MmPeerListener, MmPeer>
    {
        private readonly IPlayerCollection _playerCollection;
        private readonly IMatchMaker _matchMaker;
        private List<byte> _requiredMatchMakingProperties;
        private PendingTask _actualizeTask;
        private readonly IBackendProvider _backendProvider;
        private readonly ICreatedRoomManager _createdRoomManager;
        private readonly IPacketSender _packetSender;

        private readonly IMatchMakerServerInfoProvider _serverProvider;
        //debug
        private readonly Guid _id;

        public MmApplication(
            IShamanLogger logger,
            IApplicationConfig config,
            ISerializer serializer,
            ISocketFactory socketFactory,
            IPlayerCollection playerCollection,
            IMatchMaker matchMaker,
            IRequestSender requestSender,
            ITaskSchedulerFactory taskSchedulerFactory,
            IBackendProvider backendProvider, IPacketSender packetSender, ICreatedRoomManager createdRoomManager, IMatchMakerServerInfoProvider serverProvider) : base(logger, config, serializer,
            socketFactory, taskSchedulerFactory, requestSender)
        {
            _backendProvider = backendProvider;
            _packetSender = packetSender;
            _createdRoomManager = createdRoomManager;
            _serverProvider = serverProvider;
            _playerCollection = playerCollection;
            _matchMaker = matchMaker;
            _id = Guid.NewGuid();

            Logger?.Debug($"MmApplication constructor called. Id = {_id}");
        }

        public void SetMatchMakerProperties(List<byte> requiredMatchMakingProperties)
        {
            _requiredMatchMakingProperties = requiredMatchMakingProperties;
        }
        
        public MmServerStats GetStats()
        {
            var oldestPlayer = _playerCollection.GetOldestPlayer();
            return new MmServerStats
            {
                RegisteredServers = _serverProvider.GetGameServers().Select(s => new RegisteredServerStat
                {
                    ActualizedOn = s.ActualizedOn,
                    Address = s.Identity.ToString()
                }).ToList(),
                TotalPlayers = _playerCollection.Count(),
                OldestPlayerInMatchMaking = oldestPlayer?.StartedOn,
                CreatedRoomsCount = _createdRoomManager.GetCreatedRoomsCount()
            };
        }
        
        public override void OnStart()
        {
            var config = GetConfigAs<MmApplicationConfig>();
            
            _packetSender.Start(false);
            
            _matchMaker.Initialize(_requiredMatchMakingProperties);
            _matchMaker.Start();
            _backendProvider.Start();
            var listeners = GetListeners();
            foreach (var listener in listeners)
            {
                listener.Initialize(_matchMaker, _backendProvider, _packetSender, Config.GetAuthSecret());
            }

            _actualizeTask = TaskScheduler.ScheduleOnInterval(() =>
            {
                //send actualization request to router
                RequestSender.SendRequest<ActualizeMatchMakerResponse>(Config.GetRouterUrl(),
                    new ActualizeMatchMakerRequest(config.GameProject, config.GetServerName(), config.GetPublicName(), config.GetListenPorts().First(), config.GetAuthSecret()));
            }, 0, config.ActualizeMatchmakerIntervalMs);
        }

        public override void OnShutDown()
        {
            _playerCollection.Clear();
            _matchMaker.Stop();
        }
    }
}