using System;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.MM
{
    public class UpdateRoomStateRequest : HttpRequestBase
    {
        public Guid RoomId { get; set; }
        public int CurrentPlayerCount { get; set; }
        public RoomState State { get; set; }

        public UpdateRoomStateRequest()
            : base("updateroomstate")
        {
            
        }
        
        public UpdateRoomStateRequest(Guid roomId, int currentPlayerCount, RoomState state) 
            : this()
        {
            RoomId = roomId;
            CurrentPlayerCount = currentPlayerCount;
            State = state;
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
            typeWriter.Write(CurrentPlayerCount);
            typeWriter.Write((byte)State);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadGuid();
            CurrentPlayerCount = typeReader.ReadInt();
            State = (RoomState) typeReader.ReadByte();
        }
    }
}