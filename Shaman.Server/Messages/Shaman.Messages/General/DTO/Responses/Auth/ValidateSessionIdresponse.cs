using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses.Auth
{
    public class ValidateSessionIdResponse : HttpResponseBase
    {

        public ValidateSessionIdResponse()
            :base()
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
