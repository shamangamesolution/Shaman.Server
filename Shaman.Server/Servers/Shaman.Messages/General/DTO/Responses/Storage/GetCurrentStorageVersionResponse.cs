using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Responses.Storage
{
    public class GetCurrentStorageVersionResponse : ResponseBase
    {
        public string CurrentDatabaseVersion { get; set; }
        public string CurrentBackendVersion { get; set; }
        
        public GetCurrentStorageVersionResponse() : base(CustomOperationCode.GetCurrentStorageVersion)
        {
        }

        
        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.Write(CurrentDatabaseVersion);
            serializer.Write(CurrentBackendVersion);

        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            CurrentDatabaseVersion = serializer.ReadString();
            CurrentBackendVersion = serializer.ReadString();
        }


    }
}
