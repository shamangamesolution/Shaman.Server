using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.MM
{
    public class GetRoomListResponse : ResponseBase
    {
        public List<RoomInfo> Rooms { get; set; }

        public GetRoomListResponse() : base(Messages.ShamanOperationCode.GetRoomListResponse)
        {
            
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteList(Rooms);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            Rooms = typeReader.ReadList<RoomInfo>();
        }
    }
}