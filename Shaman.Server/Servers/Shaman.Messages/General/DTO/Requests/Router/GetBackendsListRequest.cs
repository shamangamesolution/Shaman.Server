using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests.Router
{
    public class GetBackendsListRequest : RequestBase
    {
        public GetBackendsListRequest() : base(Shaman.Common.Utils.Messages.OperationCode.GetBackendsList, BackEndEndpoints.GetBackendsList)
        {
        }

        protected override void SerializeRequestBody(ISerializer serializer)
        {
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
        }
    }
}