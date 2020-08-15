using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.RoomFlow
{
    public class CanJoinRoomResponse : HttpResponseBase
    {
        public bool CanJoin { get; set; }
        
        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(CanJoin);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            CanJoin = typeReader.ReadBool();
        }
    }
}