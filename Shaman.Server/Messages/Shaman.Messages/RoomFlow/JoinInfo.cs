using System;
using Shaman.Serialization;
using Shaman.Serialization.Messages;

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
        public bool JoinToExisting { get; set; }
        
        public JoinInfo(string serverIpAddress, ushort serverPort, Guid roomId, JoinStatus status, int currentPlayers, int maxPlayers, bool joinToExisting = false)
        {
            ServerIpAddress = serverIpAddress;
            ServerPort = serverPort;
            RoomId = roomId;
            Status = status;
            CurrentPlayers = currentPlayers;
            MaxPlayers = maxPlayers;
            JoinToExisting = joinToExisting;
        }

        public JoinInfo()
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(ServerIpAddress);
            typeWriter.Write(ServerPort);
            typeWriter.Write(RoomId);
            typeWriter.Write((byte)Status);
            typeWriter.Write(CurrentPlayers);
            typeWriter.Write(MaxPlayers);
            typeWriter.Write(JoinToExisting);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            ServerIpAddress = typeReader.ReadString();
            ServerPort = typeReader.ReadUShort();
            RoomId = typeReader.ReadGuid();
            Status = (JoinStatus) typeReader.ReadByte();
            CurrentPlayers = typeReader.ReadInt();
            MaxPlayers = typeReader.ReadInt();
            JoinToExisting = typeReader.ReadBool();
        }        
        
    }
}