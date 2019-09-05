using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Senders;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.MM.Players;
using Shaman.MM.Servers;
using Shaman.Messages;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Shaman.MM.MatchMaking
{
    public class MatchMakingGroup
    {
        public Dictionary<byte, object> Measures;
        private int _matchMakingTickMs;
        private int _totalPlayersNeeded;
        private bool _addBots;
        private bool _addOtherPlayers;
        private int _timeBeforeBotsAddedMs;
        
        private IShamanLogger _logger;
        private ITaskSchedulerFactory _taskSchedulerFactory;
        private ITaskScheduler _taskScheduler;
        private ISerializerFactory _serializerFactory;
        private IMatchMaker _matchmaker;
        private object _queueSync = new object();
        Queue<MatchMakingPlayer> _matchmakingPlayers;
        private Dictionary<byte, object> _roomProperties;
        private List<CreatedRoom> _createdRooms;
        private IPlayerCollection _playersCollection;
        private IRegisteredServerCollection _serversCollection;
        private IPacketSender _packetSender;
        private bool _isGroupWorking;
        
        public Guid Id { get; set; }
        
        public MatchMakingGroup(int totalPlayersNeeded, int matchMakingTickMs, bool addBots, bool addOtherPlayers, int timeBeforeBotsAddedMs, Dictionary<byte, object> roomProperties, Dictionary<byte, object> measures, IShamanLogger logger, ITaskSchedulerFactory taskSchedulerFactory, IMatchMaker matchmaker, IRegisteredServerCollection serverCollection, IPlayerCollection playerCollection, ISerializerFactory serializerFactory, IPacketSender packetSender)
        {
            Measures = measures;
            _matchMakingTickMs = matchMakingTickMs;
            _logger = logger;
            _taskSchedulerFactory = taskSchedulerFactory;
            _matchmaker = matchmaker;
            _serversCollection = serverCollection;
            _playersCollection = playerCollection;
            _serializerFactory = serializerFactory;
            _taskScheduler = taskSchedulerFactory.GetTaskScheduler();
            _totalPlayersNeeded = totalPlayersNeeded;
            _addBots = addBots;
            _addOtherPlayers = addOtherPlayers;
            _timeBeforeBotsAddedMs = timeBeforeBotsAddedMs;
            _createdRooms = new List<CreatedRoom>();
            _matchmakingPlayers = new Queue<MatchMakingPlayer>();
            _roomProperties = roomProperties;
            Id = Guid.NewGuid();
            _packetSender = packetSender;
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
        
        public void Start()
        {
            //TODO several threads
            _taskScheduler.ScheduleOnInterval(() =>
            {
                _logger.Debug($"MmGroup tick");
                if (_isGroupWorking)
                    return;
                
                _isGroupWorking = true;
                
                try
                {
                    lock (_queueSync)
                    {
                        var players =
                            _playersCollection.GetPlayersAndSetOnMatchmaking(Id,
                                _addOtherPlayers
                                    ? (_totalPlayersNeeded - _matchmakingPlayers.Count)
                                    : (1 - _matchmakingPlayers.Count));

                        foreach (var player in players)
                            AddPlayer(player);

                        var toAddBots = 0;
                        //check bots
                        if (_matchmakingPlayers.Count < _totalPlayersNeeded && _addBots)
                        {
                            var oldestPlayer = GetOldestPlayer();
                            if (oldestPlayer != null &&
                                (DateTime.UtcNow - oldestPlayer.AddedToMmGroupOn.Value).TotalMilliseconds >=
                                _timeBeforeBotsAddedMs)
                            {
                                toAddBots = _totalPlayersNeeded - _matchmakingPlayers.Count;
                            }
                        }

                        if (_matchmakingPlayers.Count + toAddBots >= _totalPlayersNeeded)
                        {

                            //create room
                            var server = _serversCollection.GetLessLoadedServer();
                            if (server == null)
                                return;

                            _logger.Info($"MmGroup: players found, creating room on {server.CreateRoomUrl}");


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

                            Guid roomId = server.CreateRoom(_roomProperties, _playersToSendToRoom);
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
                            var createdRoom = new CreatedRoom(roomId, 0, 10000, _playersToSendToRoom);

                            foreach (var player in _playersList)
                            {
                                _logger.Debug($"Sending join info to {player.Id}");
                                _playersCollection.SetJoinInfo(player.Id,
                                    new JoinInfo(server.Id.IpAddress, port, roomId, JoinStatus.RoomIsReady,
                                        totalHumanPlayers, _totalPlayersNeeded), true);

                                _packetSender.AddPacket(new JoinInfoEvent(player.JoinInfo), player.Peer);
                                _playersCollection.Remove(player.Id);
                            }

                            _createdRooms.Add(createdRoom);
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
    }
}