using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.DTO.Requests
{
    public class LinkExternalAccountRequest : HttpRequestBase
    {
        public int ProviderId { get; set; }
        public string ExternalId { get; set; }

        public LinkExternalAccountRequest(): base(SampleBackEndEndpoints.LinkExternalAccount)
        {
            
        }
        
        public LinkExternalAccountRequest(int providerId, string externalId) 
            :this()
        {
            ProviderId = providerId;
            ExternalId = externalId;
        }

        protected override void SerializeRequestBody(ITypeWriter serializer)
        {
            serializer.Write(ProviderId);
            serializer.Write(ExternalId);
        }

        protected override void DeserializeRequestBody(ITypeReader serializer)
        {
            ProviderId = serializer.ReadInt();
            ExternalId = serializer.ReadString();
        }
    }
}