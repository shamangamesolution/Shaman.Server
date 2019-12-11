using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Servers;
using Shaman.Messages.Extensions;

namespace Shaman.Messages.General.DTO.Requests.Router
{
    public class GetBundleUriRequest : RequestBase
    {
        public ServerIdentity ServerIdentity { get; set; }
        
        public GetBundleUriRequest() : base(CustomOperationCode.ActualizeServer, BackEndEndpoints.GetBundleUri)
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