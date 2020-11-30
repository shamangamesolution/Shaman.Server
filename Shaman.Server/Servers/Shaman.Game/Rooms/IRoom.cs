using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Messages;
using Shaman.Game.Contract;
using Shaman.Game.Contract.Stats;

namespace Shaman.Game.Rooms
{
    public interface IRoom
    {
        Guid GetRoomId();
        Task<bool> PeerJoined(IPeer peer, Dictionary<byte, object> peerProperties);
        bool PeerDisconnected(Guid sessionId, PeerDisconnectedReason reason);
        int SendToAll(MessageBase message, params Guid[] exceptions);
        int AddToSendQueue(MessageBase message, Guid sessionId);
        void ProcessMessage(ushort operationCode, MessageData message, Guid sessionId);
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
        void AddToSendQueue(MessageData messageData, ushort opCode, Guid sessionId, bool isReliable, bool isOrdered);
        void Open();
        bool IsOpen();
        void Close();

        void SendToAll(MessageData messageData, ushort opCode, bool isReliable, bool isOrdered,
            params Guid[] exceptions);

        TimeSpan ForceDestroyRoomAfter { get; }
    }
}