using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Extensions;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Router.Messages
{
    public class GetServerInfoListResponse : HttpResponseBase
    {
        public EntityDictionary<ServerInfo> ServerInfoList { get; set; }

        public GetServerInfoListResponse(): base()
        {
            
        }
        
        public GetServerInfoListResponse(EntityDictionary<ServerInfo> serverInfoList) : this()
        {
            ServerInfoList = serverInfoList;
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(ServerInfoList);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            ServerInfoList = typeReader.ReadEntityDictionary<ServerInfo>();
        }
    }
}