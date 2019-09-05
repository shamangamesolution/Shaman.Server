using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses.Auth
{
    public class ValidateSessionIdResponse : ResponseBase
    {

        public ValidateSessionIdResponse()
            :base(CustomOperationCode.ValidateSessionId)
        {

        }

        protected override void SerializeResponseBody(ISerializer serializer)
        {

        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {

        }
    }
}
