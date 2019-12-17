using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

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
