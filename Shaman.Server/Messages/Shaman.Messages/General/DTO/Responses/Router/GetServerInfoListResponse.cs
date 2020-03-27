using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.Messages.General.DTO.Responses.Router
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
            typeWriter.WriteEntityDictionary(ServerInfoList);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            ServerInfoList = typeReader.ReadEntityDictionary<ServerInfo>();
        }
    }
}