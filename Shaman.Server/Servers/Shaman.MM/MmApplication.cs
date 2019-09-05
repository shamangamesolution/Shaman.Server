using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages.General.DTO.Requests.Router;
using Shaman.Messages.General.DTO.Responses.Router;
using Shaman.MM.Configuration;
using Shaman.MM.MatchMaking;
using Shaman.MM.Peers;
using Shaman.MM.Players;
using Shaman.MM.Servers;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages.MM;
using Shaman.Messages.Stats;

namespace Shaman.MM
{
    public class MmApplication : ApplicationBase<MmPeerListener, MmPeer>
    {
        private IRegisteredServerCollection _serverCollection;
        private IPlayerCollection _playerCollection;
        private IMatchMaker _matchMaker;
        private List<MatchMakingGroup> _matchMakingGroups;
        private List<byte> _requiredMatchMakingProperties;
        private PendingTask _actualizeTask = null;
        private IBackendProvider _backendProvider;

        private IPacketSender _packetSender;
        //debug
        private Guid _id;
        
        public MmApplication(
            IShamanLogger logger, 
            IApplicationConfig config, 
            ISerializerFactory serializerFactory, 
            ISocketFactory socketFactory, 
            IRegisteredServerCollection serverCollection, 
            IPlayerCollection playerCollection, 
            IMatchMaker matchMaker,
            IRequestSender requestSender,
            ITaskSchedulerFactory taskSchedulerFactory,
            IBackendProvider backendProvider, IPacketSender packetSender)
        {
            _backendProvider = backendProvider;
            _packetSender = packetSender;
            _serverCollection = serverCollection;
            _playerCollection = playerCollection;
            _matchMaker = matchMaker;
            _id = Guid.NewGuid();
            //initialize base
            Initialize(logger, config, serializerFactory, socketFactory, taskSchedulerFactory, requestSender);
            
            Logger?.Debug($"MmApplication constructor called. Id = {_id}");
        }

        public void SetMatchMakerProperties(List<byte> requiredMatchMakingProperties)
        {
            _requiredMatchMakingProperties = requiredMatchMakingProperties;
        }
        
        public void ActualizeServer(ActualizeServerRequest request)
        {
            if (_serverCollection.Contains(request.Id))
                _serverCollection.ActualizeServer(request.Id, request.PeersCountPerPort);
            else
                _serverCollection.RegisterServer(new RegisteredServer(request.Id, RequestSender,
                    request.CreateRoomUrl, Logger));
        }

        public MmServerStats GetStats()
        {
            var oldestPlayer = _playerCollection.GetOldestPlayer();
            return new MmServerStats
            {
                RegisteredServers = _serverCollection.GetAll().Select(s => new RegisteredServerStat
                {
                    ActualizedOn = s.ActualizedOn,
                    RegisteredOn = s.RegisteredOn,
                    Address = s.Id.ToString()
                }).ToList(),
                TotalPlayers = _playerCollection.Count(),
                OldestPlayerInMatchMaking = oldestPlayer?.StartedOn 
            };
        }
        
        public override void OnStart()
        {
            var config = GetConfigAs<MmApplicationConfig>();
            
            _matchMaker.Initialize(_requiredMatchMakingProperties);
            _matchMaker.Start();
            _backendProvider.Start();
            var listeners = GetListeners();
            foreach (var listener in listeners)
            {
                listener.Initialize(_matchMaker, _backendProvider, _packetSender);
            }

            _actualizeTask = TaskScheduler.ScheduleOnInterval(() =>
            {
                //send actualization request to router
                RequestSender.SendRequest<ActualizeMatchMakerResponse>(Config.GetRouterUrl(),
                    new ActualizeMatchMakerRequest(config.GameProject, config.Name, config.GetPublicName(), config.GetListenPorts().First(), config.Secret));
            }, 0, config.ActualizeMatchmakerIntervalMs);
        }

        public override void OnShutDown()
        {
            _playerCollection.Clear();
            _serverCollection.Clear();
            _matchMaker.Clear();
        }
    }
}