using System;
using Shaman.Serialization;
using Shaman.Serialization.Extensions;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.RoomFlow
{
    public class DirectJoinRandomRoomResponse : ResponseBase
    {
        public Guid? RoomId { get; set; }
        
        public DirectJoinRandomRoomResponse() 
            : base(Messages.ShamanOperationCode.JoinRandomRoomResponse)
        {
        }

        public DirectJoinRandomRoomResponse(Guid roomId) : this()
        {
            RoomId = roomId;
        }
        
        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(RoomId);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            RoomId = typeReader.ReadNullableGuid();
        }
    }
}