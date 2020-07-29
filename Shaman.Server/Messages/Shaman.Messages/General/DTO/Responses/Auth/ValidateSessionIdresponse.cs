using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

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
