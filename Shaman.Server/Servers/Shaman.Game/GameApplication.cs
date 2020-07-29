using System;
using System.Collections.Generic;
using Shaman.Common.Contract;
using Shaman.Common.Contract.Logging;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Contract.Bundle.Stats;
using Shaman.Game.Configuration;
using Shaman.Game.Metrics;
using Shaman.Game.Peers;
using Shaman.Game.Rooms;
using Shaman.Serialization;

namespace Shaman.Game
{
    public class GameApplication : ApplicationBase<GamePeerListener, GamePeer>
    {
        private readonly IRoomManager _roomManager;
        private readonly IBackendProvider _backendProvider;
        private readonly IPacketSender _packetSender;
        private readonly IShamanMessageSenderFactory _messageSenderFactory;

        public GameApplication(IShamanLogger logger, IApplicationConfig config, ISerializer serializer,
            ISocketFactory socketFactory, ITaskSchedulerFactory taskSchedulerFactory, IRequestSender requestSender,
            IBackendProvider backendProvider, IRoomManager roomManager,
            IPacketSender packetSender, IGameMetrics gameMetrics,IShamanMessageSenderFactory messageSenderFactory) : base(logger, config, serializer, socketFactory, taskSchedulerFactory,
            requestSender, gameMetrics)
        {
            _backendProvider = backendProvider;
            _roomManager = roomManager;
            _packetSender = packetSender;
            _messageSenderFactory = messageSenderFactory;
            Logger.Debug($"GameApplication constructor called");
        }

        //some methods raised on application level - to create room from application layer
        public Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players, Guid? roomId = null)
        {
            if (properties == null)
                properties = new Dictionary<byte, object>();
            return _roomManager.CreateRoom(properties, players, roomId);            
        }
        
        public void UpdateRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            _roomManager.UpdateRoom(roomId, players);            
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
            _packetSender.Start();
            
            var config = GetConfigAs<GameApplicationConfig>();
            Logger.Info($"Game server started...");
            _backendProvider.Start();
            
            var listeners = GetListeners();
            var shamanMessageSender = _messageSenderFactory.Create(_packetSender);
            foreach (var listener in listeners)
            {
                listener.Initialize(_roomManager, _backendProvider, shamanMessageSender, Config.GetAuthSecret());
            }
        }

        public override void OnShutDown()
        {
        }
    }
}