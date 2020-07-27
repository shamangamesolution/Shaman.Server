using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Messages.General.Entity.Router;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.General.DTO.Responses.Router
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