using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.General.DTO.Requests.Auth
{
    public class GetAuthTokenRequest : HttpRequestBase
    {
        public GetAuthTokenRequest()
            :base(BackEndEndpoints.GetAuthToken)
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
