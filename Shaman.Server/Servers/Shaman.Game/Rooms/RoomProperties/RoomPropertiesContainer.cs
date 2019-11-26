using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Shaman.Common.Utils.Extensions;
using Shaman.Common.Utils.Logging;
using Shaman.Game.Contract;
using Shaman.GameBundleContract;
using Shaman.Messages;

namespace Shaman.Game.Rooms.RoomProperties
{
    public class RoomPropertiesContainer : IRoomPropertiesContainer
    {
        private IShamanLogger _logger;
        private Dictionary<Guid, Dictionary<byte, object>> _playersCameFromMatchMaker;
        private Dictionary<byte, object> _roomProperties;
        
        public RoomPropertiesContainer(IShamanLogger logger)
        {
            _logger = logger;
        }
        
        public void Initialize(Dictionary<Guid, Dictionary<byte, object>> playersCameFromMatchMaker, Dictionary<byte, object> roomProperties)
        {
            _playersCameFromMatchMaker = playersCameFromMatchMaker;
            _roomProperties = roomProperties;
            
            if (_roomProperties == null)
                _roomProperties = new Dictionary<byte, object>();
        }

        public void AddNewPlayers(Dictionary<Guid, Dictionary<byte, object>> playersCameFromMatchMaker)
        {
            foreach(var player in playersCameFromMatchMaker)
                _playersCameFromMatchMaker.Add(player.Key, player.Value);
        }

        public bool IsPlayerInMatchMakerCollection(Guid sessionId)
        {
            return _playersCameFromMatchMaker.ContainsKey(sessionId);
        }

        public void CheckIsBotForPlayers()
        {
            foreach (var player in _playersCameFromMatchMaker)
            {
                if (!player.Value.ContainsKey(PropertyCode.PlayerProperties.IsBot))
                {
                    _logger.Error($"No IsBot property in player {player.Key} record");
                    continue;
                }
            }
        }

        public int GetPlayersCount()
        {
            return _playersCameFromMatchMaker.Count - GetBotsNumber();
        }

        public int GetBotsNumber()
        {
            var botsNumber = 0;
            foreach (var player in _playersCameFromMatchMaker)
            {
                //skip bots
                if (player.Value.GetBool(PropertyCode.PlayerProperties.IsBot))
                {
                    botsNumber++;
                }
            }

            return botsNumber;
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
        
        public int GetPlayerCountToStartGame()
        {
            return _playersCameFromMatchMaker.Count;
        }

        public void RemovePlayer(Guid sessionId)
        {
            _playersCameFromMatchMaker.Remove(sessionId);
        }
    }
}