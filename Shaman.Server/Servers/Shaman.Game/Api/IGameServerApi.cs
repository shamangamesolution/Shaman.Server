using System;
using System.Collections.Generic;

namespace Shaman.Game.Api
{
    public interface IGameServerApi
    {
        /// <summary>
        /// Creates room
        /// </summary>
        /// <param name="properties">Room properties</param>
        /// <param name="players">Map of 'player id' to properties (properties may be empty)</param>
        /// <param name="roomId">If specified - will be used as room id</param>
        /// <returns></returns>
        Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players,
            Guid? roomId = null);

        /// <summary>
        /// Update player collection of room
        /// </summary>
        /// <param name="roomId">Room id</param>
        /// <param name="players">Players collection</param>
        void UpdateRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players);

        RoomInfo GetRoomInfo(Guid roomId);
    }
}