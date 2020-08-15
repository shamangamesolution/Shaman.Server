using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.MM
{
    public class UpdateRoomStateResponse : HttpResponseBase
    {
        public UpdateRoomStateResponse() 
        {
            
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
        }
    }
}