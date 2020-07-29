using System;
using System.Collections.Generic;
using Shaman.Common.Server.Applications;
using Shaman.Game.Rooms;

namespace Shaman.Game.Api
{
    class GameServerApi : IGameServerApi
    {
        private readonly GameApplication _gameApplication;
        private readonly IRoomManager _roomManager;
        
        public GameServerApi(IApplication gameApplication, IRoomManager roomManager)
        {
            _roomManager = roomManager;
            _gameApplication = (GameApplication) gameApplication;
        }

        public Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players, Guid? roomId = null)
        {
            return _gameApplication.CreateRoom(properties, players, roomId);
        }

        public void UpdateRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players)
        {
            _gameApplication.UpdateRoom(roomId, players);
        }

        public bool CanJoinRoom(Guid roomId)
        {
            var room = _roomManager.GetRoomById(roomId);
            return room != null && room.IsOpen();
        }

        public RoomInfo GetRoomInfo(Guid roomId)
        {
            var room = _roomManager.GetRoomById(roomId);
            if (room == null)
                return null;
            var stats = room.GetStats();
            return new RoomInfo
            {
                CreatedDate = stats.CreatedOn,
                IsOpen = room.IsOpen()
            };
        }
    }

    public class RoomInfo
    {
        public bool IsOpen { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}