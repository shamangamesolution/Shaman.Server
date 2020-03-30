using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Extensions;
using Shaman.Common.Utils.Logging;
using Shaman.Game.Contract;
using Shaman.Messages;

namespace Shaman.Game.Rooms.RoomProperties
{
    public class RoomPropertiesContainer : IRoomPropertiesContainer
    {
        private IShamanLogger _logger;
        private Dictionary<Guid, Dictionary<byte, object>> _playersCameFromMatchMaker;
        private Dictionary<byte, object> _roomProperties;
        private object _playersMutex = new object();
        
        public RoomPropertiesContainer(IShamanLogger logger)
        {
            _logger = logger;
        }
        
        public void Initialize(Dictionary<Guid, Dictionary<byte, object>> playersCameFromMatchMaker, Dictionary<byte, object> roomProperties)
        {
            lock (_playersMutex)
            {
                _playersCameFromMatchMaker = playersCameFromMatchMaker;
            }

            _roomProperties = roomProperties;
            
            if (_roomProperties == null)
                _roomProperties = new Dictionary<byte, object>();
        }

        public void AddNewPlayers(Dictionary<Guid, Dictionary<byte, object>> playersCameFromMatchMaker)
        {
            lock (_playersMutex)
            {
                foreach (var player in playersCameFromMatchMaker)
                {
                    _playersCameFromMatchMaker.TryAdd(player.Key, player.Value);
                }
            }
        }

        public bool IsPlayerInMatchMakerCollection(Guid sessionId)
        {
            lock (_playersMutex)
            {
                return _playersCameFromMatchMaker.ContainsKey(sessionId);
            }
        }

        public void CheckIsBotForPlayers()
        {
        }

        public int GetPlayersCount()
        {
            lock (_playersMutex)
            {
                return _playersCameFromMatchMaker.Count - GetBotsNumber();
            }
        }

        public int GetBotsNumber()
        {
            if (!_roomProperties.TryGetValue(PropertyCode.RoomProperties.TotalPlayersNeeded, out var maximumPlayers))
                return 0;
            lock (_playersMutex)
            {
                return (int) maximumPlayers - _playersCameFromMatchMaker.Count;
            }
        }

        public bool IsRoomPropertiesContainsKey(byte key)
        {
            return _roomProperties.ContainsKey(key);
        }

        public T? GetRoomProperty<T>(byte key)
            where T : struct
        {
            return (T?)(_roomProperties.GetProperty<T>(key));
        }
        
        public string GetRoomPropertyAsString(byte key)
        {
            return _roomProperties.GetString(key);
        }
        
        public int GetPlayerCountToStartGame()
        {
            lock (_playersMutex)
            {
                return _playersCameFromMatchMaker.Count;
            }
        }

        public void RemovePlayer(Guid sessionId)
        {
            lock (_playersMutex)
            {
                _playersCameFromMatchMaker.Remove(sessionId);
            }
        }
    }
}