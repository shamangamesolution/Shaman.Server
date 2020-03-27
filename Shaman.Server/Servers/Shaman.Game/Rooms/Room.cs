using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Messages;
using Shaman.Messages.MM;
using RoomStats = Shaman.Game.Contract.Stats.RoomStats;

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
        private readonly IGameModeController _gameModeController;
        private readonly RoomStats _roomStats;
        private readonly IRequestSender _requestSender;
        private readonly IRoomStateUpdater _roomStateUpdater;
        
        private RoomState _roomState = RoomState.Closed;

        private PendingTask _statTask, _updateStateTask;
        
        public Room(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IRoomManager roomManager,
            IRoomPropertiesContainer roomPropertiesContainer,
            IGameModeControllerFactory gameModeControllerFactory, IPacketSender packetSender,
            IRequestSender requestSender, Guid roomId, IRoomStateUpdater roomStateUpdater)
        {
            _logger = logger;
            _roomId = roomId;
            _roomStateUpdater = roomStateUpdater;
            _createdOn = DateTime.UtcNow;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _roomManager = roomManager;
            _roomPropertiesContainer = roomPropertiesContainer;
            _packetSender = packetSender;
            _requestSender = requestSender;

            _roomStats = new RoomStats(GetRoomId(), roomPropertiesContainer.GetPlayersCount());
            
            _gameModeController =
                gameModeControllerFactory.GetGameModeController(
                    new RoomContext(this), _taskScheduler, roomPropertiesContainer);

            _statTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                var maxQueueSIze = _packetSender.GetMaxQueueSIze();
                _roomStats.AddMaxQueueSize(maxQueueSIze);
                _roomStats.AddAvgQueueSize(_packetSender.GetAverageQueueSize());
                
            }, 0, 1000, true);

            _updateStateTask =
                _taskScheduler.ScheduleOnInterval(async () => await SendRoomStateUpdate(), 0, 2000, true); 
        }

        public TimeSpan ForceDestroyRoomAfter => _gameModeController.ForceDestroyRoomAfter;
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
            _roomStateUpdater.UpdateRoomState(GetRoomId(), _roomPlayers.Count(), _roomState, matchMakerUrl);
        }

        public void SendToAll(MessageBase message, params Guid[] exceptions)
        {
            if (exceptions == null)
                exceptions = Array.Empty<Guid>();
            
            var peers = _roomPlayers.Where(p => exceptions.All(e => e != p.Value.Peer.GetSessionId()))
                .Select(p => p.Value.Peer);
            
            var length = _packetSender.AddPacket(message, peers);
            _roomStats.TrackSentMessage(length, message.IsReliable, message.OperationCode);
        }

        public void SendToAll(MessageData messageData, ushort opCode, bool isReliable, bool isOrdered,
            params Guid[] exceptions)
        {
            if (exceptions == null)
                exceptions = Array.Empty<Guid>();

            var peers = _roomPlayers.Where(p => exceptions.All(e => e != p.Value.Peer.GetSessionId()))
                .Select(p => p.Value.Peer);
            
            _packetSender.AddPacket(peers, messageData.Buffer, messageData.Offset, messageData.Length, isReliable, isOrdered);
            _roomStats.TrackSentMessage(messageData.Length, isReliable, opCode);
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
            return _gameModeController.IsGameFinished();
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
                if (_gameModeController == null)
                {
                    _logger.Error($"GameModeController == null while peer joining");
                    return false;
                }

                var processNewPlayerResult = await _gameModeController.ProcessNewPlayer(peer.GetSessionId(), peerProperties);
                if (processNewPlayerResult && !_roomPlayers.ContainsKey(peer.GetSessionId()))
                {
                    //in case if player was disconnected during ProcessNewPlayer
                    _gameModeController.CleanupPlayer(peer.GetSessionId());
                }
                return processNewPlayerResult;;
            }
            catch (Exception ex)
            {
                _logger.Error($"PeerJoined error for player with sessionId = {peer.GetSessionId()}: {ex}");
                return false;
            }

        }

        public bool PeerDisconnected(Guid sessionId)
        {
            var peerRemoved = _roomPlayers.TryRemove(sessionId, out var roomPlayer);
            if (peerRemoved)
                _packetSender.PeerDisconnected(roomPlayer.Peer);
            _roomPropertiesContainer.RemovePlayer(sessionId);
            try
            {
                _gameModeController.CleanupPlayer(sessionId);
            }
            catch (Exception ex)
            {
                _logger.Error($"CleanUpPlayer error: {ex}");
            }
            UpdateRoomStateOnMm();
            return peerRemoved;
        }

        public void AddToSendQueue(MessageBase message, Guid sessionId)
        {
            if (!_roomPlayers.ContainsKey(sessionId))
                return;
            
            var player = _roomPlayers[sessionId];
            if (player != null)
            {
                var length = _packetSender.AddPacket(message, player.Peer);
                _roomStats.TrackSentMessage(length, message.IsReliable, message.OperationCode);
            }
            else
            {
                _logger.Error($"Trying to send message {message.GetType()} to non-existing player {sessionId}");
            }
        }
        
        public void AddToSendQueue(MessageData messageData, ushort opCode, Guid sessionId, bool isReliable, bool isOrdered)
        {
            if (!_roomPlayers.ContainsKey(sessionId))
                return;
            
            var player = _roomPlayers[sessionId];
            if (player != null)
            {
                _packetSender.AddPacket(player.Peer, messageData.Buffer, messageData.Offset, messageData.Length, isReliable, isOrdered);
                _roomStats.TrackSentMessage(messageData.Length, isReliable, opCode);
            }
            else
            {
                _logger.Error($"Trying to send message with code {opCode} to non-existing player {sessionId}");
            }
        }

        public void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId)
        {
            try
            {
                _gameModeController.ProcessMessage(operationCode, message, sessionId);
                _roomStats.TrackReceivedMessage(operationCode, message.Length, message.IsReliable);
            }
            catch (Exception ex)
            {
                _logger.Error($"Room.ProcessMessage: Error processing {operationCode} message: {ex}");
            }
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
                        player.Value.Peer.Disconnect(DisconnectReason.RoomCleanup);
                        ++removedPlayers;
                    }
                }

                _packetSender.Stop();
                _gameModeController.Cleanup();
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

        public RoomPlayer GetPlayer(Guid sessionId)
        {            
            _roomPlayers.TryGetValue(sessionId, out var player);
            return player;
        }
    }
}