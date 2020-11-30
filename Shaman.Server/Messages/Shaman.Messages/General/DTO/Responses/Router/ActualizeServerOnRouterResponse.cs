using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity.Router;

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