using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class UpdateRoomStateRequest : RequestBase
    {
        public Guid RoomId { get; set; }
        public int CurrentPlayerCount { get; set; }
        public int ClosingIn { get; set; }
        public RoomState State { get; set; }

        public UpdateRoomStateRequest()
            : base(CustomOperationCode.UpdateRoomState, "updateroomstate")
        {
            
        }
        
        public UpdateRoomStateRequest(Guid roomId, int currentPlayerCount, int closingIn, RoomState state) 
            : this()
        {
            RoomId = roomId;
            CurrentPlayerCount = currentPlayerCount;
            ClosingIn = closingIn;
            State = state;
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
            typeWriter.Write(CurrentPlayerCount);
            typeWriter.Write(ClosingIn);
            typeWriter.Write((byte)State);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadGuid();
            CurrentPlayerCount = typeReader.ReadInt();
            ClosingIn = typeReader.ReadInt();
            State = (RoomState) typeReader.ReadByte();
        }
    }
}