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
using Shaman.Game.Configuration;
using Shaman.Game.Peers;
using Shaman.Game.Rooms;
using Shaman.Messages.General.Entity.Storage;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages.MM;
using Shaman.Messages.Stats;

namespace Shaman.Game
{
    public class GameApplication : ApplicationBase<GamePeerListener, GamePeer>
    {
        private IRoomManager _roomManager;
        private ServerIdentity _serverId;
        private IBackendProvider _backendProvider;
        private int _actualizationRequestsCount = 0;
        private IStorageContainer _storageContainer;
        private IPacketSender _packetSender;
        
        public GameApplication(IShamanLogger logger, IApplicationConfig config, ISerializerFactory serializerFactory, ISocketFactory socketFactory, ITaskSchedulerFactory taskSchedulerFactory, IRequestSender requestSender, IBackendProvider backendProvider, IStorageContainer storageContainer, IRoomManager roomManager, IPacketSender packetSender)
        {
            _storageContainer = storageContainer;
            _backendProvider = backendProvider;
            _roomManager = roomManager;
            _packetSender = packetSender;
            
            Initialize(logger, config, serializerFactory, socketFactory, taskSchedulerFactory, requestSender);
            _serverId = new ServerIdentity(Config.GetPublicName(), Config.GetListenPorts().ToList());
            Logger.Debug($"GameApplication constructor called");

        }

        //single method raised on application level - to create room from application layer
        public Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            if (properties == null)
                properties = new Dictionary<byte, object>();
            return _roomManager.CreateRoom(properties, players);            
        }

        public IRoomManager GetRoomManager()
        {
            return _roomManager;
        }

        public GameServerStats GetStats()
        {
            var oldestRoom = _roomManager.GetOldestRoom();
           
            var result = new GameServerStats()
            {
                RoomCount = _roomManager.GetRoomsCount(),
                PeerCount = PeerCollection.GetPeerCount(),
                RoomsPeerCount = _roomManager.GetRoomPeerCount(),
                OldestRoomCreatedOn = oldestRoom?.GetCreatedOnDateTime()                
            };
            
            result.PeersCountPerPort = new Dictionary<ushort, int>();
            foreach(var listener in GetListeners())
                result.PeersCountPerPort.Add(listener.GetListenPort(), PeerCollection.GetPeerCount());

            return result;
        }

       
        public override void OnStart()
        {
            var config = GetConfigAs<GameApplicationConfig>();
            Logger.Info($"Game server started...");
            _backendProvider.Start();
            
            var listeners = GetListeners();
            foreach (var listener in listeners)
            {
                listener.Initialize(_roomManager, _backendProvider, _packetSender);
            }
            
            
            TaskScheduler.ScheduleOnInterval(() =>
            {
                _actualizationRequestsCount++;
                RequestSender.SendRequest<ActualizeServerResponse>(
                    GetConfigAs<GameApplicationConfig>().MatchMakerUrl,
                    new ActualizeServerRequest(_serverId, $"http://{_serverId.IpAddress}:{config.BindToPortHttp}", GetStats().PeersCountPerPort),
                    (response) =>
                    {
                        if (response.Success && _actualizationRequestsCount == 1)
                            Logger.Info("Registered on matchmaker");
                        if(!response.Success)
                            Logger.Error($"Actualization om MM failed: {response.ResultCode}|{response.Message}");
                    });
            }, 0, GetConfigAs<GameApplicationConfig>().ActualizationTimeoutMs) ;
        }

        public override void OnShutDown()
        {
        }
    }
}