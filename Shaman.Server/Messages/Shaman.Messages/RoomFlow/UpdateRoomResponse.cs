using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.RoomFlow
{
    public class UpdateRoomResponse : HttpResponseBase
    {
        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
        }
    }
}