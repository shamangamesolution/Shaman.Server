using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.Messages;
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
        private readonly ISerializer _serializer;
        private readonly IRoomPropertiesContainer _roomPropertiesContainer;
        private readonly IPacketSender _packetSender;
        private readonly IGameModeController _gameModeController;
        private readonly RoomStats _roomStats;

        public Room(IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IRoomManager roomManager, ISerializer serializer,
            IRoomPropertiesContainer roomPropertiesContainer,
            IGameModeControllerFactory gameModeControllerFactory, IPacketSender packetSender)
        {
            _logger = logger;
            _roomId = Guid.NewGuid();
            _createdOn = DateTime.UtcNow;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _roomManager = roomManager;
            _serializer = serializer;
            _roomPropertiesContainer = roomPropertiesContainer;
            _packetSender = packetSender;

            _roomStats = new RoomStats(GetRoomId(), roomPropertiesContainer.GetPlayersCount());

            if (!roomPropertiesContainer.IsRoomPropertiesContainsKey(PropertyCode.RoomProperties.GameMode))
            {
                throw new Exception($"There is no GameMode property while creating room");
            }

            // todo RW
            //throw new NotImplementedException("RW");
//            var league = (League)roomPropertiesContainer.GetRoomProperty<byte>(PropertyCode.RoomProperties.League).Value;
//
//            var gameMode = roomPropertiesContainer.IsRoomPropertiesContainsKey(PropertyCode.RoomProperties.GameMode)
//                ? (GameMode)roomPropertiesContainer.GetRoomProperty<byte>(PropertyCode.RoomProperties.GameMode).Value
//                : GetRandomGameMode(league);
            
            _gameModeController =
                gameModeControllerFactory.GetGameModeController(
                    this, _taskScheduler, roomPropertiesContainer);

            _taskScheduler.ScheduleOnInterval(() =>
            {
                var maxQueueSIze = _packetSender.GetMaxQueueSIze();
//                if (maxQueueSIze > 3)
//                {
//                    _logger.Error($"MAX SEND QUEUE: {maxQueueSIze}");
//                }
                _roomStats.AddMaxQueueSize(maxQueueSIze);
                _roomStats.AddAvgQueueSize(_packetSender.GetAverageQueueSize());
                
            }, 0, 1000, true);
        }

        public TimeSpan GetRoomTtl()
        {
            return _gameModeController.GetGameTtl();
        }

        public void UpdateRoom(Dictionary<Guid, Dictionary<byte, object>> players)
        {
            _roomPropertiesContainer.AddNewPlayers(players);
        }
        
//        private GameMode GetRandomGameMode(League league)
//        {
//            var mapLeagues = _saStorageProvider.Data.MapLeagueRules
//                .Where(r => r.IsRandomizable && r.League == league)
//                .ToList();
//
//            var selectedMode = mapLeagues[new Random().Next(mapLeagues.Count)].GameMode;
//            _logger.Error($"GetRandomGameMode: {selectedMode.ToString()} selected,  available modes: {string.Join(',', mapLeagues.Select(l=>l.GameMode.ToString()))}");
//            return selectedMode;
//        }

        public void SendToAll(MessageBase message, params Guid[] exceptions)
        {
            if (exceptions == null)
                exceptions = Array.Empty<Guid>();

            var initMsgArray = _serializer.Serialize(message);

            foreach (var player in _roomPlayers.Where(p => exceptions.All(e => e != p.Value.Peer.GetSessionId())))
            {
                AddToSendQueue(initMsgArray, player.Value.Peer, message.IsReliable, message.IsOrdered);
                //add to stats
                _roomStats.TrackSentMessage(initMsgArray.Length, message.IsReliable, message.OperationCode);
            }
        }
        public List<RoomPlayer> GetAllPlayers()
        {
            return _roomPlayers.Select(r => r.Value).ToList();
        }

        public void ConfirmedJoin(Guid sessionId)
        {
            _roomManager.ConfirmedJoin(sessionId, this);
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

        public bool PeerJoined(IPeer peer, Dictionary<byte, object> peerProperties)
        {
            if (!_roomPlayers.TryAdd(peer.GetSessionId(), new RoomPlayer(peer, peerProperties)))
            {
                _logger.Error($"Error adding player to peer collection");
                return false;
            }

            try
            {
                //find player and set wasjoined
                _gameModeController?.ProcessNewPlayer(peer.GetSessionId(), peerProperties);
                if (_gameModeController == null)
                {
                    _logger.Error($"GameModeController == null while peer joining");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"PeerJoined error for player with sessionId = {peer.GetSessionId()}: {ex}");
                return false;
            }

        }

        public void PeerLeft(Guid sessionId)
        {
            _roomPlayers.TryRemove(sessionId, out var player);
            try
            {
                _gameModeController.CleanupPlayer(sessionId);
            }
            catch (Exception ex)
            {
                _logger.Error($"CleanUpPlayer error: {ex}");
            }
        }

        public void PeerDisconnected(Guid sessionId)
        {
            _roomPlayers.TryRemove(sessionId, out var player);
            try
            {
                _gameModeController.CleanupPlayer(sessionId);
            }
            catch (Exception ex)
            {
                _logger.Error($"CleanUpPlayer error: {ex}");
            }
        }

        public void AddToSendQueue(MessageBase message, Guid sessionId)
        {
            if (!_roomPlayers.ContainsKey(sessionId))
                return;
            
            var player = _roomPlayers[sessionId];
            if (player != null)
            {
                var serialized = _serializer.Serialize(message);
                AddToSendQueue(serialized, player.Peer, message.IsReliable, message.IsOrdered);
                //add to stats
                _roomStats.TrackSentMessage(serialized.Length, message.IsReliable, message.OperationCode);
                //_taskScheduler.ScheduleOnceOnNow(() => player.Peer.Send(serialized, ));
            }
            else
            {
                _logger.Error($"Trying to send message {message.GetType()} to non-existing player {sessionId}");
            }
        }
        
        public void AddToSendQueue(byte[] bytes, IPeer peer, bool isReliable, bool isOrdered)
        {
            //_taskScheduler.ScheduleOnceOnNow(() => peer.Send(bytes, isReliable, isOrdered));
            _taskScheduler.ScheduleOnceOnNow(() =>
            {
                _packetSender.AddPacket(peer, bytes, isReliable, isOrdered);
            });

        }
        
        public void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId)
        {
            try
            {
                var result = _gameModeController.ProcessMessage(message, sessionId);
                var deserializedMessage = result.DeserializedMessage;
                _roomStats.TrackReceivedMessage(operationCode, message.Length, deserializedMessage.IsReliable);
                if (result.Handled && deserializedMessage.IsBroadcasted)
                {
                    // todo cannot pass initial array because in may be in rent and sending buffer filling in separate thread
                    SendToAll(deserializedMessage, sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Room.ProcessMessage: Error processing {operationCode} message: {ex}");
            }
        }

        public void CleanUp()
        {
            try
            {
                foreach (var player in _roomPlayers)
                {
                    player.Value.Peer.Disconnect(DisconnectReason.RoomCleanup);
                }

                _packetSender.Stop();
                _gameModeController.Cleanup();
                _taskScheduler.RemoveAll();
                _roomPlayers.Clear();
            }
            catch (Exception e)
            {
                _logger?.Error($"Error disposing room: {e}");
            }
            finally
            {
                _logger?.Error($"RoomStats: {_roomStats}");
            }
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