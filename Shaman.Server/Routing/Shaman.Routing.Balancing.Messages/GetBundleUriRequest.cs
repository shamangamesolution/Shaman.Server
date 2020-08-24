using Shaman.Common.Server.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Extensions;
using Shaman.Serialization.Messages.Http;

namespace Shaman.Router.Messages
{
    public class GetBundleUriRequest : HttpRequestBase
    {
        public ServerIdentity ServerIdentity { get; set; }
        
        public GetBundleUriRequest() : base(RouterEndpoints.GetBundleUri)
        {
            
        }
        
        public GetBundleUriRequest(ServerIdentity serverIdentity) : this()
        {
            ServerIdentity = serverIdentity;
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(ServerIdentity);
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
            ServerIdentity = typeReader.Read<ServerIdentity>();
        }
    }
}