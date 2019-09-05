using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses.Router
{
    public class ActualizeMatchMakerResponse : ResponseBase
    {
        public ActualizeMatchMakerResponse() : base(CustomOperationCode.ActualizeMatchmaker)
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