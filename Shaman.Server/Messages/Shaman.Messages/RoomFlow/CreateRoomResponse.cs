using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public class CreateRoomResponse : ResponseBase
    {
        public Guid RoomId { get; set; }
        
        public CreateRoomResponse(Guid roomId) : base(CustomOperationCode.CreateRoomFromMm)
        {
            this.RoomId = roomId;
        }

        public CreateRoomResponse() : base(CustomOperationCode.CreateRoomFromMm)
        {
            
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadGuid();
        }
    }
}