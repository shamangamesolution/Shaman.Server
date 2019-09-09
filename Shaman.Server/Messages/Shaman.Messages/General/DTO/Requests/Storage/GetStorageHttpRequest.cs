using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests.Storage
{
    public class GetStorageHttpRequest : RequestBase
    {
        public string StorageVersion { get; set; }

        public GetStorageHttpRequest()
            :base(CustomOperationCode.GetStorageHttp, BackEndEndpoints.GetStorage)
        {

        }

        public GetStorageHttpRequest(string storageVersion)
            :this()
        {
            this.StorageVersion = storageVersion;
        }

        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.Write(this.StorageVersion);
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            StorageVersion = serializer.ReadString();
        }
    }
}
