using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.DTO.Responses
{
    public class GetCurrentStorageVersionResponse : HttpResponseBase
    {
        public string CurrentDatabaseVersion { get; set; }
        public string CurrentBackendVersion { get; set; }
        
        public GetCurrentStorageVersionResponse() 
        {
        }

        
        protected override void SerializeResponseBody(ITypeWriter serializer)
        {
            serializer.Write(CurrentDatabaseVersion);
            serializer.Write(CurrentBackendVersion);

        }

        protected override void DeserializeResponseBody(ITypeReader serializer)
        {
            CurrentDatabaseVersion = serializer.ReadString();
            CurrentBackendVersion = serializer.ReadString();
        }


    }
}
