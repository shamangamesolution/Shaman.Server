using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Contract;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Bundle;
using Shaman.Messages;
using Shaman.Messages.MM;
using RoomStats = Shaman.Game.Stats.RoomStats;

namespace Shaman.Game.Rooms
{
    public class Room : IRoom
    {
        private readonly IShamanLogger _logger;
        private readonly ConcurrentDictionary<Guid, RoomPlayer> _roomPlayers = new ConcurrentDictionary<Guid, RoomPlayer>();

        private readonly Guid _roomId;
        private readonly DateTime _createdOn;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IRoomManager _roomManager;
        private readonly IRoomPropertiesContainer _roomPropertiesContainer;
        private readonly IPacketSender _packetSender;
        private readonly IRoomController _roomController;
        private readonly RoomStats _roomStats;
        private readonly IRoomStateUpdater _roomStateUpdater;
        
        private RoomState _roomState = RoomState.Closed;

        
        public Room(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IRoomManager roomManager,
            IRoomPropertiesContainer roomPropertiesContainer,
            IGameModeControllerFactory gameModeControllerFactory, IPacketSender packetSender,
            Guid roomId, IRoomStateUpdater roomStateUpdater)
        {
            _logger = logger;
            _roomId = roomId;
            _roomStateUpdater = roomStateUpdater;
            _createdOn = DateTime.UtcNow;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _roomManager = roomManager;
            _roomPropertiesContainer = roomPropertiesContainer;
            _packetSender = packetSender;

            _roomStats = new RoomStats(GetRoomId(), roomPropertiesContainer.GetPlayersCount());
            
            _roomController =
                gameModeControllerFactory.GetGameModeController(
                    new RoomContext(this), _taskScheduler, roomPropertiesContainer);

            _ = _taskScheduler.ScheduleOnInterval(() =>
            {
                var maxQueueSIze = _packetSender.GetMaxQueueSIze();
                _roomStats.AddMaxQueueSize(maxQueueSIze);
                _roomStats.AddAvgQueueSize(_packetSender.GetAverageQueueSize());
                
            }, 0, 1000, true);

            _ = _taskScheduler.ScheduleOnInterval(async () => await SendRoomStateUpdate(), 0, 2000, true); 
        }

        public TimeSpan ForceDestroyRoomAfter => _roomController.ForceDestroyRoomAfter;
        public void UpdateRoom(Dictionary<Guid, Dictionary<byte, object>> players)
        {
            _roomPropertiesContainer.AddNewPlayers(players);
        }

        public bool IsOpen()
        {
            return _roomState == RoomState.Open;
        }
        
        public void Open()
        {
            _roomState = RoomState.Open;
            UpdateRoomStateOnMm();
        }

        public void Close()
        {
            _roomState = RoomState.Closed;
            UpdateRoomStateOnMm();
        }

        private void UpdateRoomStateOnMm()
        {
            //update state on matchmaker
            _taskScheduler.ScheduleOnceOnNow(async () => await SendRoomStateUpdate());
        }

        private async Task SendRoomStateUpdate()
        {
            var matchMakerUrl =
                _roomPropertiesContainer.GetRoomPropertyAsString(PropertyCode.RoomProperties.MatchMakerUrl);
            await _roomStateUpdater.UpdateRoomState(GetRoomId(), _roomPlayers.Count(), _roomState, matchMakerUrl);
        }

        public IEnumerable<RoomPlayer> GetAllPlayers()
        {
            return _roomPlayers.Values;
        }

        public void ConfirmedJoin(Guid sessionId)
        {
            _roomManager.ConfirmedJoin(sessionId, this);
            UpdateRoomStateOnMm();
        }

        public RoomStats GetStats()
        {
            return _roomStats;
        }

        public bool IsGameFinished()
        {
            return _roomController.IsGameFinished();
        }

        public Guid GetRoomId()
        {
            return _roomId;
        }

        public async Task<bool> PeerJoined(IPeer peer, Dictionary<byte, object> peerProperties)
        {
            if (!_roomPlayers.TryAdd(peer.GetSessionId(), new RoomPlayer(peer, peerProperties)))
            {
                _logger.Error($"Error adding player to peer collection");
                return false;
            }

            try
            {
                if (_roomController == null)
                {
                    _logger.Error($"GameModeController == null while peer joining");
                    return false;
                }

                var processNewPlayerResult = await _roomController.ProcessNewPlayer(peer.GetSessionId(), peerProperties);
                if (processNewPlayerResult && !_roomPlayers.ContainsKey(peer.GetSessionId()))
                {
                    //in case if player was disconnected during ProcessNewPlayer
                    
                    // todo here we dont know true reason (plan to remove this workaround)
                    _roomController.CleanupPlayer(peer.GetSessionId(), PeerDisconnectedReason.PeerLeave, null);
                }
                return processNewPlayerResult;;
            }
            catch (Exception ex)
            {
                _logger.Error($"PeerJoined error for player with sessionId = {peer.GetSessionId()}: {ex}");
                return false;
            }

        }

        public bool PeerDisconnected(Guid sessionId, IDisconnectInfo reason)
        {
            var peerRemoved = _roomPlayers.TryRemove(sessionId, out var roomPlayer);
            if (peerRemoved)
                _packetSender.CleanupPeerData(roomPlayer.Peer);
            _roomPropertiesContainer.RemovePlayer(sessionId);
            try
            {
                _roomController.CleanupPlayer(sessionId, ResolveReason(reason.Reason), reason.Payload);
            }
            catch (Exception ex)
            {
                _logger.Error($"CleanUpPlayer error: {ex}");
            }
            UpdateRoomStateOnMm();
            return peerRemoved;
        }
        
        
        private static PeerDisconnectedReason ResolveReason(ClientDisconnectReason reason)
        {
            switch (reason)
            {
                case ClientDisconnectReason.PeerLeave:
                    return PeerDisconnectedReason.PeerLeave;
                default:
                    return PeerDisconnectedReason.ConnectionLost;
            }
        }

        public void ProcessMessage(Payload message, DeliveryOptions deliveryOptions, Guid sessionId)
        {
            var bundlePayload = new Payload(message.Buffer, message.Offset + 1, message.Length -1);
            _roomController.ProcessMessage(bundlePayload, deliveryOptions, sessionId);
            _roomStats.TrackReceivedMessage(ShamanOperationCode.Bundle, message.Length, deliveryOptions.IsReliable);
        }

        public int CleanUp()
        {
            int removedPlayers = 0;
            try
            {
                foreach (var player in _roomPlayers.ToArray())
                {
                    if (_roomPlayers.TryRemove(player.Key, out _))
                    {
                        player.Value.Peer.Disconnect(ServerDisconnectReason.RoomCleanup);
                        ++removedPlayers;
                    }
                }

                _packetSender.Stop();
                _roomController.Dispose();
                _taskScheduler.RemoveAll();
                _roomPlayers.Clear();
                //close room on matchmaker
                Close();
            }
            catch (Exception e)
            {
                _logger?.Error($"Error disposing room: {e}");
            }
            finally
            {
                _logger?.Error($"RoomStats: {_roomStats}");
            }

            return removedPlayers;
        }

        public int GetPeerCount()
        {
            return _roomPlayers.Count;
        }

        public DateTime GetCreatedOnDateTime()
        {
            return _createdOn;
        }

        public RoomPlayer FindPlayer(Guid sessionId)
        {
            _roomPlayers.TryGetValue(sessionId, out var player);
            return player;
        }
        public bool TryGetPlayer(Guid sessionId, out RoomPlayer player)
        {
            return _roomPlayers.TryGetValue(sessionId, out player);
        }
        
        private static readonly Payload BundleMessagePrefix = new Payload(ShamanOperationCode.Bundle);
        
        public void Send(Payload payload, DeliveryOptions deliveryOptions, params Guid[] sessionIds)
        {
            foreach (var sessionId in sessionIds)
            {
                if (!TryGetPlayer(sessionId, out var player))
                    return;

                _packetSender.AddPacket(player.Peer, deliveryOptions, BundleMessagePrefix, payload);
            }
        }

        public void SendToAll(Payload payload, DeliveryOptions deliveryOptions, params Guid[] exceptionSessionIds)
        {
            foreach (var roomPlayer in GetAllPlayers())
            {
                if (exceptionSessionIds.Contains(roomPlayer.Peer.GetSessionId()))
                    return;
                _packetSender.AddPacket(roomPlayer.Peer, deliveryOptions, BundleMessagePrefix, payload);
            }
        }
    }
}