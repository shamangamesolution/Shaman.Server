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
using Shaman.Game.Configuration;
using Shaman.Game.Contract;
using Shaman.Game.Contract.Stats;
using Shaman.Game.Peers;
using Shaman.Game.Rooms;
using Shaman.GameBundleContract;
using Shaman.ServerSharedUtilities.Backends;
using Shaman.Messages.MM;

namespace Shaman.Game
{
    public class GameApplication : ApplicationBase<GamePeerListener, GamePeer>
    {
        private readonly IRoomManager _roomManager;
        private readonly IBackendProvider _backendProvider;
        private readonly IPacketSender _packetSender;

        public GameApplication(IShamanLogger logger, IApplicationConfig config, ISerializer serializer,
            ISocketFactory socketFactory, ITaskSchedulerFactory taskSchedulerFactory, IRequestSender requestSender,
            IBackendProvider backendProvider, IRoomManager roomManager,
            IPacketSender packetSender) : base(logger, config, serializer, socketFactory, taskSchedulerFactory,
            requestSender)
        {
            _backendProvider = backendProvider;
            _roomManager = roomManager;
            _packetSender = packetSender;
            Logger.Debug($"GameApplication constructor called");
        }

        //some methods raised on application level - to create room from application layer
        public Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            if (properties == null)
                properties = new Dictionary<byte, object>();
            return _roomManager.CreateRoom(properties, players);            
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
            _packetSender.Start(false);
            
            var config = GetConfigAs<GameApplicationConfig>();
            Logger.Info($"Game server started...");
            _backendProvider.Start();
            
            var listeners = GetListeners();
            foreach (var listener in listeners)
            {
                listener.Initialize(_roomManager, _backendProvider, _packetSender, Config.GetAuthSecret());
            }
        }

        public override void OnShutDown()
        {
        }
    }
}