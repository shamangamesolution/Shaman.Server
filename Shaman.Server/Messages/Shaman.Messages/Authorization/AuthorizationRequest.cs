using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.Authorization
{
    public class AuthorizationRequest : RequestBase
    {
        public AuthorizationRequest() 
            : base(Messages.ShamanOperationCode.Authorization)
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