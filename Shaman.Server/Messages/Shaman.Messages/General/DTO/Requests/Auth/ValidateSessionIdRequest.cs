using System;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

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
