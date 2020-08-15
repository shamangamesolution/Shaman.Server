using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Server.Peers;
using Shaman.Common.Udp.Sockets;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;
using Shaman.Game.Stats;

namespace Shaman.Game.Rooms
{
    public interface IRoom:  IRoomSender
    {
        Guid GetRoomId();
        Task<bool> PeerJoined(IPeer peer, Dictionary<byte, object> peerProperties);
        bool PeerDisconnected(Guid sessionId, IDisconnectInfo reason);
        void ProcessMessage(Payload message, DeliveryOptions deliveryOptions, Guid sessionId);
        /// <summary>
        /// Cleans up room
        /// </summary>
        /// <returns>amount of removed players in room</returns>
        int CleanUp();
        int GetPeerCount();
        RoomPlayer FindPlayer(Guid sessionId);
        bool TryGetPlayer(Guid sessionId, out RoomPlayer player);
        DateTime GetCreatedOnDateTime();
        IEnumerable<RoomPlayer> GetAllPlayers();
        void ConfirmedJoin(Guid sessionId);
        RoomStats GetStats();
        bool IsGameFinished();
        void UpdateRoom(Dictionary<Guid, Dictionary<byte, object>> players);
        void Open();
        bool IsOpen();
        void Close();
        TimeSpan ForceDestroyRoomAfter { get; }
        bool AddPeerToRoom(IPeer peer, Dictionary<byte, object> peerProperties);
    }
}