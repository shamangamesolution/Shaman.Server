using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Routing.Common.Messages
{
    public class ActualizeServerOnMatchMakerResponse : HttpResponseBase
    {

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
        }
    }
}