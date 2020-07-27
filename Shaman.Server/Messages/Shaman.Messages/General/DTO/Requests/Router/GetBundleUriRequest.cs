using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Common.Utils.Servers;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Extensions;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Messages.General.DTO.Requests.Router
{
    public class GetBundleUriRequest : HttpRequestBase
    {
        public ServerIdentity ServerIdentity { get; set; }
        
        public GetBundleUriRequest() : base(BackEndEndpoints.GetBundleUri)
        {
            
        }
        
        public GetBundleUriRequest(ServerIdentity serverIdentity) : this()
        {
            ServerIdentity = serverIdentity;
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteEntity(ServerIdentity);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            ServerIdentity = typeReader.ReadEntity<ServerIdentity>();
        }
    }
}