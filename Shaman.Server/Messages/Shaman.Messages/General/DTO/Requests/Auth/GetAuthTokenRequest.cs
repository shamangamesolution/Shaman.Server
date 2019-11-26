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

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
        }
    }
}
