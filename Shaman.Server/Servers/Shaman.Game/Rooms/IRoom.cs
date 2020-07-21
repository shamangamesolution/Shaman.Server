using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Sockets;
using Shaman.Game.Contract;
using Shaman.Game.Contract.Stats;
using Shaman.Game.Stats;

namespace Shaman.Game.Rooms
{
    public interface IRoom
    {
        Guid GetRoomId();
        Task<bool> PeerJoined(IPeer peer, Dictionary<byte, object> peerProperties);
        bool PeerDisconnected(Guid sessionId, IDisconnectInfo reason);
        void ProcessMessage(MessageData message, Guid sessionId);
        /// <summary>
        /// Cleans up room
        /// </summary>
        /// <returns>amount of removed players in room</returns>
        int CleanUp();
        int GetPeerCount();
        RoomPlayer GetPlayer(Guid sessionId);
        DateTime GetCreatedOnDateTime();
        IEnumerable<RoomPlayer> GetAllPlayers();
        void ConfirmedJoin(Guid sessionId);
        RoomStats GetStats();
        bool IsGameFinished();
        void UpdateRoom(Dictionary<Guid, Dictionary<byte, object>> players);
        void Send(MessageData messageData, SendOptions sendOptions, params Guid[] sessionIds);
        void SendToAll(MessageData messageData, SendOptions sendOptions, params Guid[] exceptionSessionIds);
        void Open();
        bool IsOpen();
        void Close();
        TimeSpan ForceDestroyRoomAfter { get; }
    }

}