using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.MM.Players;
using Shaman.Messages;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;
using Shaman.MM.Metrics;
using Shaman.MM.Providers;

namespace Shaman.MM.MatchMaking
{
    public class MatchMakingGroup
    {
        private const int TIME_TO_KEEP_CREATED_ROOM_SEC = 1800;
        
        public readonly Dictionary<byte, object> Measures;
        private readonly int _matchMakingTickMs;
        private readonly int _totalPlayersNeeded;
        private readonly bool _addBots;
        private readonly bool _addOtherPlayers;
        private readonly int _timeBeforeBotsAddedMs;

        private readonly IMatchMakerServerInfoProvider _serverProvider;
        private ICreatedRoomManager _createdRoomManager;
        private IShamanLogger _logger;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private ISerializer _serializer;
        private IMatchMaker _matchmaker;
        private object _queueSync = new object();
        Queue<MatchMakingPlayer> _matchmakingPlayers;
        private Dictionary<byte, object> _roomProperties;
        private IPlayerCollection _playersCollection;
//        private IRegisteredServerCollection _serversCollection;
        private IPacketSender _packetSender;
        private readonly IMmMetrics _mmMetrics;
        private bool _isGroupWorking;
        private PendingTask _clearTask, _mainTask;
        public Guid Id { get; set; }
        private int _roomClosingInMs;

        public MatchMakingGroup(int totalPlayersNeeded, int matchMakingTickMs, bool addBots, bool addOtherPlayers,
            int timeBeforeBotsAddedMs, int roomClosingInMs, Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures,
            IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IMatchMaker matchmaker,
            IPlayerCollection playerCollection,
            ISerializer serializer, IPacketSender packetSender, IMmMetrics mmMetrics, ICreatedRoomManager createdRoomManager, IMatchMakerServerInfoProvider serverProvider)
        {
            Measures = measures;
            _matchMakingTickMs = matchMakingTickMs;
            _logger = logger;
            _taskSchedulerFactory = taskSchedulerFactory;
            _matchmaker = matchmaker;
            _playersCollection = playerCollection;
            _serializer = serializer;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _totalPlayersNeeded = totalPlayersNeeded;
            _addBots = addBots;
            _addOtherPlayers = addOtherPlayers;
            _timeBeforeBotsAddedMs = timeBeforeBotsAddedMs;
            _roomClosingInMs = roomClosingInMs;
            _matchmakingPlayers = new Queue<MatchMakingPlayer>();
            _roomProperties = roomProperties;
            Id = Guid.NewGuid();
            _packetSender = packetSender;
            _mmMetrics = mmMetrics;
            _createdRoomManager = createdRoomManager;
            _serverProvider = serverProvider;
        }

        private void AddPlayer(MatchMakingPlayer player)
        {
            lock (_queueSync)
            {
                player.AddedToMmGroupOn = DateTime.UtcNow;
                _matchmakingPlayers.Enqueue(player);
            }
        }

        private MatchMakingPlayer GetPlayer()
        {
            lock (_queueSync)
            {
                if (_matchmakingPlayers.Count == 0)
                    return null;
                else
                {
                    return _matchmakingPlayers.Dequeue();
                }
            }
        }
        
        private void SendJoinInfoToCurrentMatchmakingGroup()
        {
            lock (_queueSync)
            {
                foreach (var player in _matchmakingPlayers)
                {
                    _logger.Debug($"Sending prejoin info to {player.Id}");
                    _playersCollection.SetJoinInfo(player.Id,
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

        private void AddToNewRoom()
        {
            //create room
            //var server = _serversCollection.GetLessLoadedServer();
            var server = _serverProvider.GetLessLoadedServer();
            if (server == null)
                return;

            _logger.Info($"MmGroup: players found, creating room on {server.Identity}");

            //prepare players dict to send to room
            var _playersToSendToRoom = new Dictionary<Guid, Dictionary<byte, object>>();
            var _playersList = new List<MatchMakingPlayer>();

            var totalHumanPlayers = _matchmakingPlayers.Count;
            //move player from inner collection to new list
            for (int i = 0; i < _totalPlayersNeeded; i++)
            {
                var player = GetPlayer();
                if (player != null)
                {
                    _playersList.Add(player);
                    //add additional property
                    if (!player.Properties.ContainsKey(PropertyCode.PlayerProperties.IsBot))
                        player.Properties.Add(PropertyCode.PlayerProperties.IsBot, false);
                    _playersToSendToRoom.Add(player.SessionId, player.Properties);
                }
                else
                {
                    _playersToSendToRoom.Add(Guid.NewGuid(), new Dictionary<byte, object>()
                    {
                        {PropertyCode.PlayerProperties.IsBot, true}
                    });
                }
            }

            Guid roomId = _serverProvider.CreateRoom(server.Id, _roomProperties, _playersToSendToRoom);
            if (roomId == Guid.Empty)
            {
                foreach (var player in _playersList)
                {
                    _playersCollection.SetOnMatchmaking(player.Id, false);
                    _logger.Info($"Sending matchmaking failed info to {player.Id}");
                    _playersCollection.SetJoinInfo(player.Id,
                        new JoinInfo("", 0, Guid.Empty, JoinStatus.MatchMakingFailed, 0, 0), false);
                    _packetSender.AddPacket(new JoinInfoEvent(player.JoinInfo), player.Peer);
                }

                return;
            }

            var port = server.GetLessLoadedPort();

            //create room to allow joining during game
            var createdRoom = new CreatedRoom(roomId, 0, _roomClosingInMs, _playersToSendToRoom, server, _addOtherPlayers);
            _createdRoomManager.AddCreatedRoom(createdRoom);

            foreach (var player in _playersList)
            {
                _logger.Debug($"Sending join info to {player.Id}");
                _playersCollection.SetJoinInfo(player.Id,
                    new JoinInfo(server.Identity.Address, port, roomId, JoinStatus.RoomIsReady,
                        totalHumanPlayers, _totalPlayersNeeded), true);

                _packetSender.AddPacket(new JoinInfoEvent(player.JoinInfo), player.Peer);
                _playersCollection.Remove(player.Id);
            }
        }
        
        private void AddToExistingRoom(CreatedRoom createdRoom)
        {
            var _playersToSendToRoom = new Dictionary<Guid, Dictionary<byte, object>>();
            var _playersList = new List<MatchMakingPlayer>();
            var totalHumanPlayers = _matchmakingPlayers.Count;

            for (int i = 0; i < totalHumanPlayers; i++)
            {
                var player = GetPlayer();
                if (player != null)
                {
                    _playersList.Add(player);
                    //add additional property
                    player.Properties.Add(PropertyCode.PlayerProperties.IsBot, false);
                    _playersToSendToRoom.Add(player.SessionId, player.Properties);
                }
            }
            
            //update room with new players data
            _serverProvider.UpdateRoom(createdRoom.Server.Id, _playersToSendToRoom, createdRoom.Id);

            var port = createdRoom.Server.GetLessLoadedPort();
            
            //add new players to room
            createdRoom.AddPlayers(_playersToSendToRoom);
            
            foreach (var player in _playersList)
            {
                _logger.Debug($"Sending join info to {player.Id}");
                _playersCollection.SetJoinInfo(player.Id,
                    new JoinInfo(createdRoom.Server.Identity.Address, port, createdRoom.Id, JoinStatus.RoomIsReady,
                        totalHumanPlayers, _totalPlayersNeeded, true), true);

                _packetSender.AddPacket(new JoinInfoEvent(player.JoinInfo), player.Peer);
                _playersCollection.Remove(player.Id);
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
                            _playersCollection.GetPlayersAndSetOnMatchmaking(Id,
                                _addOtherPlayers
                                    ? (_totalPlayersNeeded - matchmakingPlayersCount)
                                    : (1 - matchmakingPlayersCount));

                        foreach (var player in players)
                            AddPlayer(player);
                        
                        var oldestPlayer = GetOldestPlayer();

                        var toAddBots = 0;
                        //check bots
                        if (matchmakingPlayersCount < _totalPlayersNeeded && _addBots)
                        {
                            if (oldestPlayer != null &&
                                (DateTime.UtcNow - oldestPlayer.AddedToMmGroupOn.Value).TotalMilliseconds >=
                                _timeBeforeBotsAddedMs)
                            {
                                toAddBots = _totalPlayersNeeded - matchmakingPlayersCount;
                            }
                            else
                            {
                                if (matchmakingPlayersCount > 0)
                                {
                                    //we can not add bots because of timer, but we can try to find existing room for players
                                    var room = _createdRoomManager.GetRoomForPlayers(matchmakingPlayersCount);
                                    if (room != null)
                                    {
                                        AddToExistingRoom(room);
                                        TrackMmTime(oldestPlayer);
                                        return;
                                    }
                                }
                            }
                        }

                        if (matchmakingPlayersCount + toAddBots >= _totalPlayersNeeded)
                        {
                            AddToNewRoom();
                            TrackMmTime(oldestPlayer);
                        }
                        else
                        {
                            //send matchmaking status
                            SendJoinInfoToCurrentMatchmakingGroup();
                        }

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