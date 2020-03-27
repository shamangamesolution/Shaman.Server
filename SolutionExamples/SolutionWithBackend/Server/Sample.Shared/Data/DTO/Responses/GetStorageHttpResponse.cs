using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.DTO.Responses
{
    public class GetStorageHttpResponse : HttpResponseBase
    {
        public byte[] SerializedAndCompressedStorage { get; set; }

        public GetStorageHttpResponse() 
        {
        }

        protected override void SerializeResponseBody(ITypeWriter serializer)
        {
            serializer.Write(SerializedAndCompressedStorage);
        }

        protected override void DeserializeResponseBody(ITypeReader serializer)
        {
            SerializedAndCompressedStorage = serializer.ReadBytes();
        }
    }
}
