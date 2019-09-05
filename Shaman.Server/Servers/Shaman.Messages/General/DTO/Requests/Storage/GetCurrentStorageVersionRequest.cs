using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests.Storage
{
    public class GetCurrentStorageVersionRequest : RequestBase
    {
        public GetCurrentStorageVersionRequest()
         :base(CustomOperationCode.GetCurrentStorageVersion, BackEndEndpoints.GetStorageVersion)
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
