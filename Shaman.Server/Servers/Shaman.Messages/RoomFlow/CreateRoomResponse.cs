using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.RoomFlow
{
    public class CreateRoomResponse : ResponseBase
    {
        public Guid RoomId { get; set; }
        
        public CreateRoomResponse(Guid roomId) : base(Messages.CustomOperationCode.CreateRoom)
        {
            this.RoomId = roomId;
        }

        public CreateRoomResponse() : base(Messages.CustomOperationCode.CreateRoom)
        {
            
        }

        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.WriteBytes(RoomId.ToByteArray());
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            this.RoomId = new Guid(serializer.ReadBytes());
        }
    }
}