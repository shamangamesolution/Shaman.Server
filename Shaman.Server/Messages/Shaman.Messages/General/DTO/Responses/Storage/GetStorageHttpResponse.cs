using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses.Storage
{
    public class GetStorageHttpResponse : ResponseBase
    {
        public byte[] SerializedAndCompressedStorage { get; set; }

        public GetStorageHttpResponse() : base(CustomOperationCode.GetStorageHttp)
        {
        }

        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.Write(SerializedAndCompressedStorage);
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            SerializedAndCompressedStorage = serializer.ReadBytes();
        }
    }
}
