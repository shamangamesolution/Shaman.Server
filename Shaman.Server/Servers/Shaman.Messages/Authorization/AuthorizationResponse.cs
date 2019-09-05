using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.Authorization
{
    public class AuthorizationResponse : ResponseBase
    {
        
        public AuthorizationResponse() 
            : base(Messages.CustomOperationCode.Authorization)
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