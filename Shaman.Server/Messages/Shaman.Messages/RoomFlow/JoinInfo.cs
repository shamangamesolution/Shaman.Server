using System;
using System.Net;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public enum JoinStatus : byte
    {
        OnMatchmaking = 1,
        RoomIsReady = 2,
        MatchMakingFailed = 3
    }
    public class JoinInfo : EntityBase
    {
        public string ServerIpAddress { get; set; }
        public ushort ServerPort { get; set; }
        public Guid RoomId { get; set; }
        public JoinStatus Status { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; set; }
        
        public JoinInfo(string serverIpAddress, ushort serverPort, Guid roomId, JoinStatus status, int currentPlayers, int maxPlayers)
        {
            ServerIpAddress = serverIpAddress;
            ServerPort = serverPort;
            RoomId = roomId;
            Status = status;
            CurrentPlayers = currentPlayers;
            MaxPlayers = maxPlayers;
        }

        public JoinInfo()
        {
        }

        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.WriteString(ServerIpAddress);
            serializer.WriteUShort(ServerPort);
            serializer.WriteBytes(RoomId.ToByteArray());
            serializer.WriteByte((byte)Status);
            serializer.WriteInt(CurrentPlayers);
            serializer.WriteInt(MaxPlayers);
            
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            ServerIpAddress = serializer.ReadString();
            ServerPort = serializer.ReadUShort();
            RoomId = new Guid(serializer.ReadBytes());
            Status = (JoinStatus) serializer.ReadByte();
            CurrentPlayers = serializer.ReadInt();
            MaxPlayers = serializer.ReadInt();
        }        
        
    }
}