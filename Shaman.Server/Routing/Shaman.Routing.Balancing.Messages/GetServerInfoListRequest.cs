using Shaman.Serialization;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Routing.Balancing.Messages
{
    public class GetServerInfoListRequest : HttpRequestBase
    {
        public bool ActualOnly { get; set; }

        public GetServerInfoListRequest() : base(RouterEndpoints.GetServerInfoList)
        {
            
        }
        
        public GetServerInfoListRequest(bool actualOnly = false) : this()
        {
            ActualOnly = actualOnly;
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(ActualOnly);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            ActualOnly = typeReader.ReadBool();
        }
    }
}