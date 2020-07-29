using System;
using Shaman.Common.Utils.Serialization;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.General.DTO.Responses.Auth
{
    public class GetAuthTokenResponse : HttpResponseBase
    {
        public Guid AuthToken { get; set; }

        public GetAuthTokenResponse()
            :base()
        {
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(this.AuthToken);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            AuthToken = typeReader.ReadGuid();
        }
    }
}
