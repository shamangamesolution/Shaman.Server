using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses.Auth
{
    public class GetAuthTokenResponse : ResponseBase
    {
        public Guid AuthToken { get; set; }

        public GetAuthTokenResponse()
            :base(CustomOperationCode.GetAuthToken)
        {
        }

        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.WriteBytes(this.AuthToken.ToByteArray());

        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            AuthToken = new Guid(serializer.ReadBytes());

        }
    }
}
