using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.DTO.Requests
{
    public class GetStorageHttpRequest : HttpRequestBase
    {
        public string StorageVersion { get; set; }

        public GetStorageHttpRequest()
            :base(SampleBackEndEndpoints.GetStorage)
        {

        }

        public GetStorageHttpRequest(string storageVersion)
            :this()
        {
            this.StorageVersion = storageVersion;
        }

        protected override void SerializeRequestBody(ITypeWriter serializer)
        {
            serializer.Write(this.StorageVersion);
        }

        protected override void DeserializeRequestBody(ITypeReader serializer)
        {
            StorageVersion = serializer.ReadString();
        }
    }
}
