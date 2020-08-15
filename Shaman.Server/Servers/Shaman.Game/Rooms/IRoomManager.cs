using System;
using System.Collections.Generic;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Sockets;
using Shaman.Contract.Common;

namespace Shaman.Game.Rooms
{
    public interface IRoomManager
    {
        Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players,
            Guid? roomId);
        void UpdateRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players);
        List<IRoom> GetAllRooms();
        int GetRoomsCount();
        IRoom GetRoomBySessionId(Guid sessionId);
        bool IsInRoom(Guid sessionId);
        void ProcessMessage(ushort operationCode, Payload message, DeliveryOptions deliveryOptions, IPeer peer);
        Dictionary<Guid, int> GetRoomPeerCount();
        IRoom GetOldestRoom();
        void ConfirmedJoin(Guid sessionId, IRoom room);
        void PeerDisconnected(IPeer peer, IDisconnectInfo info);
        IRoom GetRoomById(Guid id);
    }
}