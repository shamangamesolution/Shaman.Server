using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

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