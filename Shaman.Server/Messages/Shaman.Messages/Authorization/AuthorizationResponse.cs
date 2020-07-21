using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

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