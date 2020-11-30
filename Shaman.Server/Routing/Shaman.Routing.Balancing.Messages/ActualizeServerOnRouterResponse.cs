using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Routing.Balancing.Messages
{
    public class ActualizeServerOnRouterResponse : HttpResponseBase
    {
        public ActualizeServerOnRouterResponse() : base()
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