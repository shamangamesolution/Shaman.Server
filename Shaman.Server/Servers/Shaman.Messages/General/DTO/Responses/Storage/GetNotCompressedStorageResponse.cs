using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses.Storage
{
    public class GetNotCompressedStorageResponse : ResponseBase
    {
        public byte[] SerializedStorage { get; set; }

        public GetNotCompressedStorageResponse() : base(CustomOperationCode.GetNotCompressedStorage)
        {
        }

        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.Write(SerializedStorage);
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            SerializedStorage = serializer.ReadBytes();
        }
    }
}
