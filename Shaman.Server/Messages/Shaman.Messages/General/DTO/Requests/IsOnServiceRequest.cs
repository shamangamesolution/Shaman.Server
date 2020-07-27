using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.General.DTO.Requests
{
    public class IsOnServiceRequest : HttpRequestBase
    {
        public IsOnServiceRequest()
            :base(BackEndEndpoints.IsOnService)
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
