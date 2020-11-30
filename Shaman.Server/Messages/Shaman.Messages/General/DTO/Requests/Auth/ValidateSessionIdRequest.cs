using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests.Auth
{
    public class ValidateSessionIdRequest : HttpRequestBase
    {
        public string Secret { get; set; }
        public ValidateSessionIdRequest()
            :base(BackEndEndpoints.ValidateSessionId)
        {

        }

        public ValidateSessionIdRequest(Guid token, string secret)
            :this()
        {
            this.SessionId = token;
            this.Secret = secret;
        }
        
        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(Secret);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            Secret = typeReader.ReadString();
        }
    }
}
