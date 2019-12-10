using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Game.Contract;
using Shaman.MM.Players;
using Shaman.Messages;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Managers;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;
using Shaman.MM.Rooms;

namespace Shaman.MM.MatchMaking
{
    public class MatchMakingGroup
    {
        public Guid Id { get; set; }

        private readonly Dictionary<byte, object> _measures;
        private readonly IShamanLogger _logger;
        private readonly IPlayersManager _playersManager;
        private readonly ITaskScheduler _taskScheduler;
        private readonly ISerializer _serializer;
        private readonly IMmMetrics _mmMetrics;
        private readonly IRoomManager _roomManager;
        private readonly IBotManager _botManager;
        private readonly IPacketSender _packetSender;

        private readonly Dictionary<byte, object> _roomProperties;
        private readonly int _matchMakingTickMs;
        private readonly int _totalPlayersNeeded;
        private readonly bool _addBots;
        private readonly bool _addOtherPlayers;
        private readonly int _timeBeforeBotsAddedMs;

        private object _queueSync = new object();
        private Queue<MatchMakingPlayer> _matchmakingPlayers;
        private bool _isGroupWorking;
        private PendingTask _clearTask, _mainTask;

        public MatchMakingGroup(Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures,
            IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IPlayersManager playersManager,
            IPacketSender packetSender, IMmMetrics mmMetrics, IRoomManager roomManager, IBotManager botManager)
        {
            _measures = measures;
            _logger = logger;
            _playersManager = playersManager;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _matchmakingPlayers = new Queue<MatchMakingPlayer>();
            _roomProperties = roomProperties;
            Id = Guid.NewGuid();
            _packetSender = packetSender;
            _mmMetrics = mmMetrics;
            _roomManager = roomManager;
            _botManager = botManager;

            //get properties from dict
            if (!roomProperties.TryGetValue(PropertyCode.RoomProperties.MatchMakingTick, out var tickProperty))
                throw new Exception($"MatchMakingGroup ctr error: there is no MatchMakingTick property");
            if (!roomProperties.TryGetValue(PropertyCode.RoomProperties.TotalPlayersNeeded, out var totalPlayersProperty))
                throw new Exception($"MatchMakingGroup ctr error: there is no TotalPlayersNeeded property");
            if (!roomProperties.TryGetValue(PropertyCode.RoomProperties.ToAddBots, out var addBotsProperty))
                throw new Exception($"MatchMakingGroup ctr error: there is no ToAddBots property");
            if (!roomProperties.TryGetValue(PropertyCode.RoomProperties.ToAddOtherPlayers, out var addOthersProperty))
                throw new Exception($"MatchMakingGroup ctr error: there is no ToAddOtherPlayers property");
            if (!roomProperties.TryGetValue(PropertyCode.RoomProperties.TimeBeforeBotsAdded, out var timeBeforeBotsProperty))
                throw new Exception($"MatchMakingGroup ctr error: there is no TimeBeforeBotsAdded property");
            
            _matchMakingTickMs = (int)tickProperty;
            _totalPlayersNeeded = (int)totalPlayersProperty;
            _addBots = (bool)addBotsProperty;
            _addOtherPlayers = (bool)addOthersProperty;
            _timeBeforeBotsAddedMs = (int)timeBeforeBotsProperty;
        }
        
        private void AddPlayer(MatchMakingPlayer player)
        {
            lock (_queueSync)
            {
                player.AddedToMmGroupOn = DateTime.UtcNow;
                _matchmakingPlayers.Enqueue(player);
            }
        }

        private void SendJoinInfoToCurrentMatchmakingGroup()
        {
            lock (_queueSync)
            {
                foreach (var player in _matchmakingPlayers)
                {
                    _logger.Debug($"Sending prejoin info to {player.Id}");
                    _playersManager.SetJoinInfo(player.Id,
                        new JoinInfo("", 0, Guid.Empty, JoinStatus.OnMatchmaking, _matchmakingPlayers.Count(),
                            _totalPlayersNeeded), false);
                    
                    _packetSender.AddPacket(new JoinInfoEvent(player.JoinInfo), player.Peer);
                }
            }
        }

        private MatchMakingPlayer GetOldestPlayer()
        {
            lock (_queueSync)
            {
                return _matchmakingPlayers.OrderBy(p => p.AddedToMmGroupOn).FirstOrDefault();
            }
        }

        private void TrackMmTime(MatchMakingPlayer oldestPlayer)
        {
            if (oldestPlayer != null && oldestPlayer.AddedToMmGroupOn.HasValue)
            {
                _mmMetrics.TrackMmCompleted((long) (DateTime.UtcNow - oldestPlayer.AddedToMmGroupOn.Value).TotalMilliseconds);
            }
            else
            {
                _logger.Error($"Oldest player not found when tracking MM time");
            }
        }

        private bool NeedToAddBots(MatchMakingPlayer oldestPlayer)
        {
            if (oldestPlayer?.AddedToMmGroupOn == null)
                return false;
            
            return _matchmakingPlayers.Count < _totalPlayersNeeded && _addBots && (DateTime.UtcNow - oldestPlayer.AddedToMmGroupOn.Value).TotalMilliseconds >= _timeBeforeBotsAddedMs;
        }

        private void SendMatchmakingFailed(MatchMakingPlayer player)
        {
            _logger.Error($"Sending matchmaking failed info to {player.Id}");
            _playersManager.SetOnMatchmaking(player.Id, false);
            _playersManager.SetJoinInfo(player.Id,
                new JoinInfo("", 0, Guid.Empty, JoinStatus.MatchMakingFailed, 0, 0), false);
            _packetSender.AddPacket(new JoinInfoEvent(player.JoinInfo), player.Peer);
        }

        private void SendMatchMakingComplete(MatchMakingPlayer player, JoinRoomResult result)
        {
            _logger.Debug($"Sending join info to {player.Id}");
            _playersManager.SetJoinInfo(player.Id,
                new JoinInfo(result.Address, result.Port, result.RoomId, JoinStatus.RoomIsReady,
                    _matchmakingPlayers.Count, _totalPlayersNeeded, true), true);
            _packetSender.AddPacket(new JoinInfoEvent(player.JoinInfo), player.Peer);
            _playersManager.Remove(player.Id);
        }

        private void ProcessJoinResult(JoinRoomResult result, MatchMakingPlayer oldestPlayer)
        {
            switch (result.Result)
            {
                case RoomOperationResult.OK:
                    foreach (var player in _matchmakingPlayers)
                        SendMatchMakingComplete(player,result);
                    break;
                case RoomOperationResult.ServerNotFound:
                case RoomOperationResult.JoinRoomError:
                    foreach (var player in _matchmakingPlayers)
                        SendMatchmakingFailed(player);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                            
            _matchmakingPlayers.Clear();
            if (oldestPlayer != null)
                TrackMmTime(oldestPlayer);
            _isGroupWorking = false;
        }
        
        public void Start()
        {
            //schedule main task
            _mainTask = _taskScheduler.ScheduleOnInterval(() =>
            {
                _logger.Debug($"MmGroup tick");
                if (_isGroupWorking)
                    return;
                
                _isGroupWorking = true;
                
                try
                {
                    lock (_queueSync)
                    {
                        var matchmakingPlayersCount = _matchmakingPlayers.Count;
                        
                        var players =
                            _playersManager.GetPlayers(Id,
                                _addOtherPlayers
                                    ? (_totalPlayersNeeded - matchmakingPlayersCount)
                                    : (1 - matchmakingPlayersCount));

                        foreach (var player in players)
                        {
                            _playersManager.SetOnMatchmaking(player.Id, true);
                            AddPlayer(player);
                        }
                        
                        matchmakingPlayersCount = _matchmakingPlayers.Count;

                        //if noone in collection - continue
                        if (matchmakingPlayersCount == 0)
                        {
                            _isGroupWorking = false;
                            return;
                        }
                        
                        var oldestPlayer = GetOldestPlayer();

                        //try to add to existing room
                        var room = _roomManager.GetRoom(Id, matchmakingPlayersCount);
                        JoinRoomResult result = null;
                        
                        if (room != null)
                        {
                            //join to existing
                            result = _roomManager.JoinRoom(room.Id,
                                _matchmakingPlayers.ToDictionary(key => key.SessionId, value => value.Properties),
                                _measures);
                        }
                        else if (NeedToAddBots(oldestPlayer))
                        {
                            var bots = _botManager.GetBots(_totalPlayersNeeded - matchmakingPlayersCount);
                            //add new room
                            result = _roomManager.CreateRoom(Id,
                                _matchmakingPlayers.ToDictionary(key => key.SessionId, value => value.Properties), bots,
                                _roomProperties, _measures);
                        }

                        if (result != null)
                            ProcessJoinResult(result, oldestPlayer);
                        else
                            SendJoinInfoToCurrentMatchmakingGroup();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"MM tick error: {ex}");
                }
                finally
                {
                    _isGroupWorking = false;
                }
            }, 0, _matchMakingTickMs);
        }

        public void Stop()
        {
           _taskScheduler.RemoveAll();
        }
    }
}