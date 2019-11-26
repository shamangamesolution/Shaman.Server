using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests.Storage
{
    public class GetNotCompressedStorageRequest : RequestBase
    {

        public GetNotCompressedStorageRequest()
            :base(CustomOperationCode.GetNotCompressedStorage, BackEndEndpoints.GetNotCompressedStorage)
        {

        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
        }
    }
}
