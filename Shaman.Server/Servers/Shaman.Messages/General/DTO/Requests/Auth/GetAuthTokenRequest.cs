using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests.Auth
{
    public class GetAuthTokenRequest : RequestBase
    {
        public GetAuthTokenRequest()
            :base(CustomOperationCode.GetAuthToken, BackEndEndpoints.GetAuthToken)
        {

        }

        protected override void SerializeRequestBody(ISerializer serializer)
        {
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
        }
    }
}
