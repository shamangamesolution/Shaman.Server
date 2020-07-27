using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.Authorization
{
    public class AuthorizationResponse : ResponseBase
    {
        
        public AuthorizationResponse() 
            : base(Messages.ShamanOperationCode.AuthorizationResponse)
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