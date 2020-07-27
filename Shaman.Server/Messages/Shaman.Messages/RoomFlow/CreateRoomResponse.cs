using System;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.RoomFlow
{
    public class CreateRoomResponse : HttpResponseBase
    {
        public Guid RoomId { get; set; }
        
        public CreateRoomResponse(Guid roomId) 
        {
            this.RoomId = roomId;
        }

        public CreateRoomResponse()
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