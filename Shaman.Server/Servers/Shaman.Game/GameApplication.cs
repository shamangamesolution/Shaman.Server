using System;
using System.Collections.Generic;
using Shaman.Common.Http;
using Shaman.Common.Server.Applications;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Protection;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle.Stats;
using Shaman.Contract.Common.Logging;
using Shaman.Game.Metrics;
using Shaman.Game.Peers;
using Shaman.Game.Rooms;
using Shaman.Serialization;

namespace Shaman.Game
{
    public class GameApplication : ApplicationBase<GamePeerListener, GamePeer>
    {
        private readonly IRoomManager _roomManager;
        private readonly IPacketSender _packetSender;
        private readonly IShamanMessageSenderFactory _messageSenderFactory;

        public GameApplication(IShamanLogger logger, IApplicationConfig config, ISerializer serializer,
            IServerTransportLayerFactory serverTransportLayerFactory, ITaskSchedulerFactory taskSchedulerFactory, IRequestSender requestSender,
            IRoomManager roomManager, IPacketSender packetSender, IGameMetrics gameMetrics,
            IShamanMessageSenderFactory messageSenderFactory, IProtectionManager protectionManager ) :
            base(logger, config, serializer, serverTransportLayerFactory, taskSchedulerFactory, requestSender, gameMetrics, protectionManager)
        {
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
            
            Logger.Info($"Game server started...");
            
            var listeners = GetListeners();
            var shamanMessageSender = _messageSenderFactory.Create(_packetSender);
            foreach (var listener in listeners)
            {
                listener.Initialize(_roomManager, shamanMessageSender, Config.AuthSecret);
            }
        }

        protected override void TrackMetrics()
        {
            base.TrackMetrics();
            ServerMetrics.TrackSendersCount(nameof(GameApplication), _packetSender.GetKnownPeersCount());
        }

        public override void OnShutDown()
        {
        }
    }
}