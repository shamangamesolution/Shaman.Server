using System;
using System.Collections.Generic;
using Shaman.Common.Server.Applications;

namespace Shaman.Game.Api
{
    class GameServerApi : IGameServerApi
    {
        private readonly GameApplication _gameApplication;

        public GameServerApi(IApplication gameApplication)
        {
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
    }
}